using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClothStruct 
{
	/// 布料质点的定义
	public class ClothPoint
	{
		/// 布料顶点的 Transform 
		public Transform transform;

		///  现在的位置
		public Vector3 position { get { return transform.position; } set { transform.position = value; } }

		/// 以前的位置
		public Vector3 prePosition;

		///  质点运动计算的权重(0.0 - 1.0)
		public float weight;



		public ClothPoint(Transform transform, float weight)
		{
			this.transform = transform;
			this.prePosition = transform.position;
			this.weight = weight;
		}
	}

	///副碰撞球，实现防止布料自相交
	public class AuxiliarySphere
	{
		public Transform transform;

		/// 副碰撞球现在的位置
		public Vector3 position { get { return transform.position; } set { transform.position = value; } }

		/// 副碰撞球以前的位置
		public Vector3 prePosition;

		/// <summary>
		/// 副碰撞球的半径
		/// </summary>
		public float radius;

		public AuxiliarySphere(Transform transform, float radius)
		{
			this.transform = transform;
			this.radius = radius;
		}
	}




	/// <summary>
	/// 约束力
	/// </summary>
	public class ClothConstraint
	{
		/// <summary> 质点1 </summary>
		public ClothPoint p1;

		/// <summary> 质点2 </summary>
		public ClothPoint p2;

		/// <summary> 自然长 </summary>
		public float restLength;   // 自然长

		/// <summary> 弹簧种类 </summary>
		public SpringType type;

		public ClothConstraint(ClothPoint p1, ClothPoint p2, float restLength, SpringType type)
		{
			this.p1 = p1;
			this.p2 = p2;
			this.restLength = restLength;
			this.type = type;
		}
	}

	/// <summary>
	/// 弹簧的种类
	/// </summary>
	public enum SpringType
	{
		Structual,  // 拉伸弹簧
		Shear,      // 剪切弹簧
		Bending,    // 弯曲弹簧
	}

	/// <summary>
	/// 传递值结构
	/// </summary>
	public struct ConcatInfo
	{
		public Vector3 position;
		public Vector3 normal;
	}

	/// <summary>
	/// 刚体
	/// </summary>
	public struct RigidbodyDesc
	{
		public float mass;
		public float bounciness;
		public Vector3 velocity;
	}

	
}
