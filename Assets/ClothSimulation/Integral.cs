using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClothStruct;
using Hash;

namespace Integral
{
    public class MyIntegral : MonoBehaviour
    {
		/// <summary>
		/// 通过Veltex积分求质点位置
		/// </summary>
		/// <param name="dt"></param>
		public void Integral(float dt, ClothPoint[] clothPoints, Vector3 gravity,float massPoint, float drag)
		{
			// TODO 进行并行计算
			for (int i = 0; i < clothPoints.Length; i++)
			{
				var p = clothPoints[i];

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
	}
}

