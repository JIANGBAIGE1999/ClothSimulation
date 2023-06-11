using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClothStruct;

namespace IterativeConstraint
{
    public class Iterative : MonoBehaviour
    {
		/// <summary>
		/// 满足弹簧约束
		/// </summary>
		/// <param name="count">迭代次数</param>
		public void SatisfyConstraint(int count, float dt, ClothConstraint[] constraints, float structuralShrink, float structuralStretch, float shrinkShrink, float shrinkStretch, float bendingShrink, float bendingStretch, float massPoint)
		{
			var ddt = dt / count;

			for (int ite = 0; ite < count; ite++)
			{
				for (int i = 0; i < constraints.Length; i++)
				{
					if (constraints[i] == null) { continue; }

					// 如果是固定点的话跳过计算
					if (constraints[i].p1.weight <= 0 && constraints[i].p2.weight <= 0)
					{
						continue;
					}

					float shrink = 0;   // 耐伸长性
					float stretch = 0;  // 抗收缩性

					// 拉伸弹簧
					if (constraints[i].type == SpringType.Structual)
					{
						shrink = structuralShrink;
						stretch = structuralStretch;
					}

					// 剪切弹簧
					else if (constraints[i].type == SpringType.Shear)
					{
						shrink = shrinkShrink;
						stretch = shrinkStretch;
					}

					// 弯曲弹簧
					else
					{
						shrink = bendingShrink;
						stretch = bendingStretch;
					}

					// 计算弹簧力
					// 计算弹簧的伸长
					var diff = (constraints[i].p2.position - constraints[i].p1.position);
					var mag = diff.magnitude;
					var f_scalar = mag - constraints[i].restLength;
					f_scalar = f_scalar >= 0 ? f_scalar * shrink : f_scalar * stretch;
					var f = f_scalar * (diff / Mathf.Abs(mag));

					// 计算位移
					var v = (f * ddt) / massPoint;  // 对于此刻，以初始速度为0进行计算。

					// p1的权重
					var p1w = constraints[i].p1.weight / (constraints[i].p1.weight + constraints[i].p2.weight);

					// p2的权重
					var p2w = constraints[i].p2.weight / (constraints[i].p1.weight + constraints[i].p2.weight);

					// 更新位置
					constraints[i].p1.position = constraints[i].p1.position + (p1w * v * ddt);
					constraints[i].p2.position = constraints[i].p2.position - (p2w * v * ddt);
				}
			}
		}
	}

}
