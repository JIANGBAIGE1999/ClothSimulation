using Unity.Jobs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollisionDecection;
using ClothStruct;

public class BoneMassSpringClothTest : MonoBehaviour
{
	MassSpringCloth massSpringCloth = default;
	

	[SerializeField]
	Transform rootBone = default;

	[SerializeField]
	int div = 16;

	[Header("副碰撞球的半径"), Range(0, 3.0f)]
	public float sphereRadius = 0.5f;

	[Header("质点数量密度半径"), Range(0, 2.0f)]
	public float massRadius = 0.5f;

	[Header("质点密度"), Range(0, 10.0f)]
	public float density = 1.0f;

	ClothPoint[] clothPoints = default;
	///AuxiliarySphere[] auxiliarySpheres = default;
	ClothConstraint[] constraints;


	// Start is called before the first frame update
	void Start()
	{
		massSpringCloth = GetComponent<MassSpringCloth>();

		// Initialize
		// 质点
		clothPoints = new ClothPoint[div * div];
		///auxiliarySpheres = new AuxiliarySphere[div * div];

		int px = 0;
		int py = 0;
		foreach (Transform bone in rootBone.transform)
		{
			//Debug.Log(bone.name);
			//基于顶点密度的质量生成算法
			var weight = CalculateVertexMass(bone, py);
			clothPoints[(py * div) + px] = new ClothPoint(bone, weight);
			///auxiliarySpheres[(py * div) + px] = new AuxiliarySphere(bone, sphereRadius);
			px++;
			if (px >= div)
			{
				px = 0;
				py++;
			}
		}



		// Constraint
		constraints = new ClothConstraint[(div * div) * 6];
		for (int y = 0; y < div; y++)
		{
			for (int x = 0; x < div; x++)
			{
				// 拉伸弹簧
				// 左
				if (x > 0)
				{
					constraints[(div * y * 6) + (x * 6)] = GetConstraint(ref clothPoints[(div * y) + x], ref clothPoints[(div * y) + x - 1], SpringType.Structual);
				}
				else
				{
					constraints[(div * y * 6) + (x * 6)] = null;
				}

				// 上
				if (y + 1 < div)
				{
					constraints[(div * y * 6) + ((x * 6) + 1)] = GetConstraint(ref clothPoints[(div * y) + x], ref clothPoints[(div * (y + 1)) + x], SpringType.Structual);
				}
				else
				{
					constraints[(div * y * 6) + ((x * 6) + 1)] = null;
				}


				// 剪切弹簧
				// 左上
				if (x > 0 && y + 1 < div)
				{
					constraints[(div * y * 6) + ((x * 6) + 2)] = GetConstraint(ref clothPoints[(div * y) + x], ref clothPoints[(div * (y + 1)) + x - 1], SpringType.Shear);
				}
				else
				{
					constraints[(div * y * 6) + ((x * 6) + 2)] = null;
				}

				// 右上
				if (x + 1 < div && y + 1 < div)
				{
					constraints[(div * y * 6) + ((x * 6) + 3)] = GetConstraint(ref clothPoints[(div * y) + x], ref clothPoints[(div * (y + 1)) + x + 1], SpringType.Shear);
				}
				else
				{
					constraints[(div * y * 6) + ((x * 6) + 3)] = null;
				}


				// 弯曲弹簧
				// 左
				if (x > 1)
				{
					constraints[(div * y * 6) + ((x * 6) + 4)] = GetConstraint(ref clothPoints[(div * y) + x], ref clothPoints[(div * y) + x - 2], SpringType.Bending);
				}
				else
				{
					constraints[(div * y * 6) + ((x * 6) + 4)] = null;
				}

				// 上
				if (y + 1 < div - 1)
				{
					constraints[(div * y * 6) + ((x * 6) + 5)] = GetConstraint(ref clothPoints[(div * y) + x], ref clothPoints[(div * (y + 2)) + x], SpringType.Bending);
				}
				else
				{
					constraints[(div * y * 6) + ((x * 6) + 5)] = null;
				}
			}
		}

		massSpringCloth.Initialize(clothPoints, constraints);
	}

	private ClothConstraint GetConstraint(
		ref ClothPoint p1,
		ref ClothPoint p2,
		SpringType type)
	{
		if (p1 == null || p2 == null) { return null; }

		return new ClothConstraint(
			p1,
			p2,
			(p2.position - p1.position).magnitude,
			type);
	}

	float CalculateVertexMass(Transform i, int py) 
	{
		massSpringCloth = GetComponent<MassSpringCloth>();

		
		float weight = 0;
		var cnt = 0;
		foreach (Transform j in rootBone.transform)
		{
			Vector3 diff = i.position - j.position;
			var mag = diff.magnitude;
			if (mag <= massRadius)
			{
				cnt++;
			}
		}
		weight = cnt * density;
		if (py == 0)
		{
			weight = 0.0f;
		}
		   
		return weight;

	}
}
