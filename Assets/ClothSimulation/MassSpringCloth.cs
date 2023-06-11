using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollisionDecection;
using ClothStruct;
//using Integral;
using IterativeConstraint;
using Hash;

public class MassSpringCloth : MonoBehaviour
{
	#region Parameter
	/// <summary>
	/// 初始化MySphere碰撞类，有碰撞检测和处理功能
	/// </summary>
	MySphere mySphere = new MySphere();
	/// <summary>
	/// 初始化 MyBox碰撞类，有碰撞检测和处理功能
	/// </summary>
	MyBox myBox = new MyBox();
	/// <summary>
	/// 初始化MyCapsule碰撞类，有碰撞检测和处理功能
	/// </summary>
	MyCapsule myCapsule = new MyCapsule();
	/// <summary>
	/// 初始化积分类，计算布料各顶点运动位置
	/// </summary>
	//MyIntegral myIntegral = new MyIntegral();
	/// <summary>
	/// 迭代类，计算布料各顶点受力
	/// </summary>
	Iterative myIterative = new Iterative();
	
	#endregion



	#region クラスメンバー
	/// <summary> 质点的集合 </summary>
	public ClothPoint[] clothPoints { get; private set; }

	/// <summary> 副碰撞球的集合 </summary>
	public AuxiliarySphere[] auxiliarySpheres { get; private set; }

	/// <summary> 约束的集合 </summary>
	public ClothConstraint[] constraints { get; private set; }


	/// <summary> 布料的质量 </summary>
	[Header("布料的质量")]
	public float mass = 0.5f;

	/// <summary> 质点的质量 </summary>
	float massPoint;

	/// <summary> 重力 </summary>
	[Header("重力")]
	public Vector3 gravity = new Vector3(0, -9.8f, 0);

	/// <summary> 空气阻力 </summary>
	[Header("空气阻力"), Range(0f, 1.0f)]
	public float drag = 0.1f;

	/// <summary> 迭代处理次数 </summary>
	[Header("迭代处理次数")]
	public int relaxationCount = 5;

	/// <summary> 拉伸弹簧的伸展力 </summary>
	[Header("拉伸弹簧的伸展力"), Range(0, 20.0f)]
	public float structuralShrink = 10.0f;

	/// <summary> 拉伸弹簧的收缩力 </summary>
	[Header("拉伸弹簧的收缩力"), Range(0, 20.0f)]
	public float structuralStretch = 10.0f;

	/// <summary> 剪切弹簧的伸展力 </summary>
	[Header("剪切弹簧的伸展力"), Range(0, 20.0f)]
	public float shrinkShrink = 0.1f;

	/// <summary> 剪切弹簧的收缩力 </summary>
	[Header("剪切弹簧的收缩力"), Range(0, 20.0f)]
	public float shrinkStretch = 0.1f;

	/// <summary> 弯曲弹簧的伸展力 </summary>
	[Header("弯曲弹簧的伸展力"), Range(0, 20.0f)]
	public float bendingShrink = 10.0f;

	/// <summary> 弯曲弹簧的收缩力 </summary>
	[Header("弯曲弹簧的收缩力"), Range(0, 20.0f)]
	public float bendingStretch = 0.1f;

	[Header("空间哈希表的相关属性")]
	public int maxNumObjects;
	public int tableSize;
	public int[] cellCount;//cellCount存储在particleMap中开始寻找的下标
	public int[] particleMap;//particleMap是粒子查询数组
	public int[] queryIds;
	public int querySize;
	public int[] firstAdjId;//firstAdjId是追踪首次索引到adjIds的位置
	public int[] adjIds;//adjIds是与某一特定id相邻的所有顶点id打包成一个密集的数组

	[Header("顶点半径"), Range(0, 1.0f)]
	public float spacing;


	/// <summary> 球的位置 </summary>
	[Header("球")]
	public SphereCollider sphereCollider;

	[Header("球刚体")]
	public Rigidbody sphererb;

	RigidbodyDesc sphere;


	[Header("盒子")]
	public BoxCollider boxCollider;

	[Header("盒子刚体")]
	public Rigidbody boxrb;

	RigidbodyDesc box;

	[Header("胶囊体")]
	public CapsuleCollider capsuleCollider;

	[Header("胶囊体刚体")]
	public Rigidbody capsulerb;

	RigidbodyDesc capsule;

	

	#endregion

	/// <summary>
	/// 初始化
	/// </summary>
	/// <param name="clothPoints"></param>
	/// <param name="constraints"></param>
	public void Initialize(ClothPoint[] clothPoints, ClothConstraint[] constraints)
	{
		this.clothPoints = clothPoints;
		this.constraints = constraints;
		massPoint = mass / clothPoints.Length;

		maxNumObjects = clothPoints.Length;
		this.spacing = Mathf.Abs(clothPoints[1].position.x - clothPoints[0].position.x)/2;///设置定点半径，因为布料骨骼是均匀分布，所以这次设置为两个顶点间距离的一半
		this.tableSize = 5 * maxNumObjects;
		this.cellCount = new int[this.tableSize + 1];
		this.particleMap = new int[maxNumObjects];
		this.queryIds = new int[maxNumObjects];
		this.querySize = 0;
		this.firstAdjId = new int[maxNumObjects + 1];
		this.adjIds = new int[10 * maxNumObjects];
	}


	/// <summary>
	/// 初始化空间哈希表，用这个结构防止布料自相交
	/// </summary>
	SpaticalHash spaticalHash = new SpaticalHash( );
	


	/// <summary>
	/// 利用Verlet积分计算布料各个顶点的运动轨迹
	/// </summary>

	private void Integral(float dt, ClothPoint[] clothPoints, Vector3 gravity, float massPoint, float drag)
	{
		// TODO 进行并行计算
		for (int i = 0; i < clothPoints.Length; i++)
		{
			//计算两个顶点间的安全距离
			float spacing2 = spacing * spacing;

			var p = clothPoints[i];
			// 如果是固定点的话跳过计算
			if (p.weight == 0)
				continue;

			///以下是防止布料自相交的过程
			int id0 = i;
			///确定和顶点i相邻的顶点
			var first = firstAdjId[i];
			var last =  firstAdjId[i + 1];




			for (int j = first; j < last; j++)
			{
				int id1 = adjIds[j];
				if (clothPoints[id1].weight == 0.0)
					continue;

				float dist2 = (clothPoints[id1].position - clothPoints[id0].position).magnitude * (clothPoints[id1].position - clothPoints[id0].position).magnitude;
				//Debug.Log(dist2);
				//Debug.Log(spacing2);
				if (dist2 > spacing2 || dist2 == 0.0)///如果两点间距离大于安全距离或为自己和自己碰撞，就不做处理
					continue;
				//Debug.Log(dist2);
				float restDist2 = spacing2;///因为本次布料骨骼的分布均匀，所以用两个顶点骨骼间的距离当作弹簧的自然长度

				float minDist = spacing;
				if (dist2 >= restDist2)///如果两点间距离大于弹簧自然长度，就不做处理
					continue;
				if (restDist2 < spacing2)///如果两点间距离小于弹簧自然长度说明自相交，将两碰撞顶点分别向碰撞方向的相反方向移动一半的相交距离就行
					minDist = (float)Mathf.Sqrt(restDist2);
				Debug.Log("检测到自碰撞");
				// 位置修正
				//计算两个粒子之间的距离
				float dist = (clothPoints[id1].position - clothPoints[id0].position).magnitude;

				//调整粒子的位置，使它们之间的距离等于两个副碰撞球的半径之和
				float correctionDistance = 2 * spacing - dist;

				//计算应当沿哪个方向进行调整
				Vector3 correctionDirection = (clothPoints[id1].position - clothPoints[id0].position).normalized;

				//分别调整两个粒子的位置
				clothPoints[id0].position = clothPoints[id0].position + correctionDirection * correctionDistance / 2;
				clothPoints[id1].position = clothPoints[id1].position - correctionDirection * correctionDistance / 2;

			}

			// 求质点的初始速度
			var v0 = (p.position - p.prePosition) / dt;

			// 更新前回位置
			p.prePosition = p.position;

			// 计算由力引起的位移
			// 力
			var f = (gravity * massPoint) + (v0 * drag * -1.0f);

			// 根据动量的变化计算速度
			var v = (gravity * dt) + v0;

			// 计算新的位置
			p.position = p.position + ((v * dt) * p.weight);
		}
	}








	//暴力算法防止布料自相交，但复杂度过高无法使用
	private void selfInteraction()
	{
		for (int i = 0; i < auxiliarySpheres.Length; i++)
		{
			for (int j = 0; j < auxiliarySpheres.Length; j++)
			{
				//计算两个粒子之间的距离
				Vector3 diff = auxiliarySpheres[i].transform.position - auxiliarySpheres[j].transform.position;
				float dist = diff.magnitude;

				//如果距离小于两个副碰撞球的半径之和，说明碰撞发生
				if (dist < 2 * auxiliarySpheres[i].radius)
				{
					//调整粒子的位置，使它们之间的距离等于两个副碰撞球的半径之和
					float correctionDistance = 2 * auxiliarySpheres[i].radius - dist;

					//计算应当沿哪个方向进行调整
					Vector3 correctionDirection = diff.normalized;

					//分别调整两个粒子的位置
					auxiliarySpheres[i].position = auxiliarySpheres[i].transform.position + correctionDirection * correctionDistance / 2;
					auxiliarySpheres[j].position = auxiliarySpheres[j].transform.position - correctionDirection * correctionDistance / 2;
					clothPoints[i].position = auxiliarySpheres[i].position + correctionDirection * correctionDistance / 2;
					clothPoints[j].position = auxiliarySpheres[j].position - correctionDirection * correctionDistance / 2;
				}
			}
		}
	}

	


	private void Awake()
    {
		///初始化球碰撞体质量，弹性系数和初速度
		sphere.mass = sphererb.mass;
		sphere.bounciness = 0.6f;
		sphere.velocity = sphererb.velocity;

		///初始化盒子碰撞体质量，弹性系数和初速度
		box.mass = boxrb.mass;
		box.bounciness = 0.6f;
		box.velocity = boxrb.velocity;

		///初始化胶囊碰撞体质量，弹性系数和初速度
		capsule.mass = capsulerb.mass;
		capsule.bounciness = 0.6f;
		capsule.velocity = capsulerb.velocity;

		
	}
    private void Start()
    {
		///设置将空间划分的网格的大小,在这时设置为顶点半径的二倍，即两个顶点间的距离
		var maxDist = spacing * 2;
		///生成布料顶点对应的空间哈希表
		///Debug.Log(spacing);
		spaticalHash.CreateHash(clothPoints, ref cellCount, ref particleMap, tableSize, spacing);
		spaticalHash.QueryAll(clothPoints, maxDist, maxNumObjects, ref adjIds, ref firstAdjId, ref cellCount, ref queryIds, ref querySize, ref particleMap, spacing, tableSize);
	}

    private void FixedUpdate()
	{
		//myIntegral.Integral(Time.deltaTime, clothPoints, gravity, massPoint, drag);
		Integral(Time.deltaTime, clothPoints, gravity, massPoint, drag);
		myIterative.SatisfyConstraint(relaxationCount, Time.deltaTime, constraints, structuralShrink, structuralStretch, shrinkShrink, shrinkStretch,bendingShrink, bendingStretch, massPoint);
		mySphere.detectingSphereCollisions(sphereCollider, clothPoints,ref sphere,ref sphererb);
		myBox.detectingBoxCollisions(boxCollider, clothPoints, ref box, ref boxrb);
		myCapsule.detectingCapsuleCollision(capsuleCollider, clothPoints,ref capsule,ref capsulerb);
		//selfInteraction();

	}
}
