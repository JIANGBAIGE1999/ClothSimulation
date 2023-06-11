using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClothStruct;

namespace CollisionDecection
{

	public class MySphere
	{
		public bool GetClosestSurfacePoint(SphereCollider sphereCollider, Vector3 p, out ConcatInfo concatInfo)
		{
			concatInfo = default(ConcatInfo);
			Vector3 spherePos = sphereCollider.transform.position;/// 球体的中心位置
			Vector3 c2p = p - spherePos;
			float d2 = c2p.magnitude;

			float sphereRadius = sphereCollider.radius;/// 球体的半径
			if (d2 < sphereRadius)
			{
				concatInfo.normal = c2p.normalized;
				concatInfo.position = spherePos + concatInfo.normal * sphereRadius;
				return true;

			}
			else
			{
				return false;
			}
		}



		public void EnableCollisionConstraint(ClothPoint[] clothPoints, int index, ref ConcatInfo concatInfo, ref RigidbodyDesc rigidbody)
		{

			var m1 = rigidbody.mass;
			if (m1 > 0)
			{
				var bounciness = rigidbody.bounciness;
				var m0 = clothPoints[index].weight;
				var dt = Time.deltaTime;
				var v0 = (clothPoints[index].position - clothPoints[index].prePosition) / dt;
				var v0Normal = Vector3.Dot(v0, concatInfo.normal) * concatInfo.normal;
				var v1Normal = Vector3.Dot(rigidbody.velocity, concatInfo.normal) * concatInfo.normal;

				var v0NormalNew = (bounciness + 1) * m1 * v1Normal + v0Normal * (m0 - bounciness * m1);///冲量守恒
				var v1NormalNew = (bounciness + 1) * m0 * v0Normal + v1Normal * (m1 - bounciness * m0);///冲量守恒

				v0NormalNew /= (m0 + m1);
				v1NormalNew /= (m0 + m1);

				rigidbody.velocity = v1NormalNew - v1Normal;

			}

		}

		public void detectingSphereCollisions(SphereCollider sphereCollider, ClothPoint[] clothPoints, ref RigidbodyDesc sphere, ref Rigidbody sphererb)
		{

			for (int i = 0; i < clothPoints.Length; i++)
			{
				ConcatInfo concatInfo;
				/// 点与球体的碰撞检测
				if (GetClosestSurfacePoint(sphereCollider, clothPoints[i].position, out concatInfo))
				{
					clothPoints[i].position = concatInfo.position;/// 发生了碰撞，所以将点移动到球体的表面
					RigidbodyDesc tmp = sphere;
					///计算碰撞体速度
					EnableCollisionConstraint(clothPoints, i, ref concatInfo, ref tmp);
					sphere = tmp;
					///对碰撞体施加力
					sphererb.AddForce(sphere.velocity, ForceMode.VelocityChange);
					//Debug.Log(sphere.velocity);
				}
			}
		}

	}

	public class MyBox
	{
		/// <summary>
		/// 阶跃函数
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		private Vector3 step(Vector3 a, Vector3 b) 
		{
			Vector3 result = new Vector3(judge(a.x, b.x), judge(a.y, b.y), judge(a.z, b.z));
			return result;
		}
		private float judge(float a, float b) 
		{
			if (a < b)
				return 1.0f;
			else
				return 0.0f;
		}
		

		private bool All(Vector3 p)
		{
			if (p.x > 0 && p.y > 0 && p.z > 0)
				return true;
			else
				return false;
		}
		public bool GetClosestSurfacePoint(Vector3 p, BoxCollider boxCollider, out ConcatInfo concatInfo)
		{
			var o = boxCollider.center - boxCollider.size * 0.5f; //min作为原点
			var localToWorld = boxCollider.transform.localToWorldMatrix;
			o = localToWorld.MultiplyPoint(o);
			p = p - o;

			var dx = localToWorld.MultiplyVector(new Vector3(boxCollider.size.x, 0, 0));
			var dy = localToWorld.MultiplyVector(new Vector3(0, boxCollider.size.y, 0));
			var dz = localToWorld.MultiplyVector(new Vector3(0, 0, boxCollider.size.z));

			Vector3 projOnAxis = new Vector3(
				Vector3.Dot(p, dx.normalized),
				Vector3.Dot(p, dy.normalized),
				Vector3.Dot(p, dz.normalized)
			);///可以处理旋转

			Vector3 axisLength = new Vector3(dx.magnitude, dy.magnitude, dz.magnitude);
			Vector3 side = step(axisLength * 0.5f, projOnAxis); // >0.5 => 1 | <0.5 => 0
			Vector3 signedDist = Vector3.Scale((Vector3.one - side * 2) ,(projOnAxis - Vector3.Scale(side ,axisLength)));
	
			bool inside = All(signedDist);

			if (inside)
			{
				var dst = signedDist.x;
				var axisNormalized = dx.normalized;
				var axisW = dx.magnitude;
				var sideFlag = side.x;
				var axisIndex = 0;
				if (signedDist.y < dst)
				{
					dst = signedDist.y;
					axisIndex = 1;
					sideFlag = side.y;
					axisNormalized = dy.normalized;
					axisW = dy.magnitude;
				}
				if (signedDist.z < dst)
				{
					dst = signedDist.z;
					sideFlag = side.z;
					axisIndex = 2;
					axisNormalized = dz.normalized;
					axisW = dz.magnitude;
				}
				concatInfo = new ConcatInfo();

				

				concatInfo.normal = sideFlag == 1 ? axisNormalized : -axisNormalized;
				var offset = (projOnAxis[axisIndex] - sideFlag * axisW);
				concatInfo.position = o + p - axisNormalized * offset;
				return true;
			}
			else
			{
				concatInfo = default;
				return false;
			}
		}

		public void EnableCollisionConstraint(ClothPoint[] clothPoints, int index, ref ConcatInfo concatInfo, ref RigidbodyDesc rigidbody)
		{
			//Debug.Log(concatInfo.normal);
			var m1 = rigidbody.mass;
			
			if (m1 > 0)
			{
				var bounciness = rigidbody.bounciness;
				var m0 = clothPoints[index].weight;
				var dt = Time.deltaTime;
				var v0 = (clothPoints[index].position - clothPoints[index].prePosition) / dt;
				var v0Normal = Vector3.Dot(v0, concatInfo.normal) * concatInfo.normal;
				var v1Normal = Vector3.Dot(rigidbody.velocity, concatInfo.normal) * concatInfo.normal;
				var v0NormalNew = (bounciness + 1) * m1 * v1Normal + v0Normal * (m0 - bounciness * m1);///冲量守恒
				var v1NormalNew = (bounciness + 1) * m0 * v0Normal + v1Normal * (m1 - bounciness * m0);///冲量守恒

				v0NormalNew /= (m0 + m1);
				v1NormalNew /= (m0 + m1);

				

				rigidbody.velocity = v1NormalNew - v1Normal;
			}

		}
		public void detectingBoxCollisions(BoxCollider boxCollider, ClothPoint[] clothPoints, ref RigidbodyDesc box, ref Rigidbody boxrb)
		{
			for (int i = 0; i < clothPoints.Length; i++)
			{
				ConcatInfo concatInfo;
				// 检查布料的每一个顶点是否在AABB内
				if (GetClosestSurfacePoint(clothPoints[i].position, boxCollider, out concatInfo))
				{

					//Debug.Log(clothPoints[i].position);
					// 更新布料的顶点
					clothPoints[i].position = concatInfo.position;/// 发生了碰撞，所以将点移动到盒子的表面
					RigidbodyDesc tmp = box;
					///计算碰撞体速度
					EnableCollisionConstraint(clothPoints, i, ref concatInfo, ref tmp);
					box = tmp;
					///对碰撞体施加力
					boxrb.AddForce(box.velocity, ForceMode.VelocityChange);
					//Debug.Log(box.velocity);
				}
			}

		}
	}

	public class MyCapsule
	{

		public static bool GetClosestSurfacePoint(Vector3 p, CapsuleCollider capsuleCollider, out ConcatInfo concatInfo)
		{
			concatInfo = default(ConcatInfo);

			var localToWorld = capsuleCollider.transform.localToWorldMatrix;

			var o = capsuleCollider.center;
			o[capsuleCollider.direction] = o[capsuleCollider.direction] - capsuleCollider.height * 0.5f + capsuleCollider.radius;
			o = localToWorld.MultiplyPoint3x4(o);//c0作为原点

			var axis = capsuleCollider.transform.up;
			var localScale = capsuleCollider.transform.localScale;
			float axisScale = localScale.y;
			float radiusScale = localScale.x;
			if (capsuleCollider.direction == 0)
			{
				axis = capsuleCollider.transform.right;
				axisScale = localScale.x;
				radiusScale = localScale.y;
			}
			else if (capsuleCollider.direction == 2)
			{
				axis = capsuleCollider.transform.forward;
				axisScale = localScale.z;
				radiusScale = localScale.x;
			}

			var radius = capsuleCollider.radius * radiusScale;
			var axisW = (capsuleCollider.height - 2 * capsuleCollider.radius) * axisScale;

			p = p - o;
			var proj = Vector3.Dot(p, axis); //p点在轴上的投影
			if (proj < -radius || proj > axisW + radius)
			{
				return false;
			}
			var r2 = radius * radius;
			if (proj >= 0 && proj <= axisW)
			{
				//轴上投影在圆柱体之间
				var dist2 = Vector3.Dot(p, p) - proj * proj; //计算p到轴的垂直距离平方
				if (dist2 < r2)
				{
					var q = axis * proj;
					concatInfo.normal = (p - q).normalized;
					concatInfo.position = o + q + concatInfo.normal * radius;
					return true;
				}
				else
				{
					return false;
				}
			}
			if (proj >= -radius && proj < 0)
			{
				//轴上投影处于原点附近
				var c2p = p;
				if (Vector3.Dot(c2p, c2p) < r2)
				{
					concatInfo.normal = c2p.normalized;
					concatInfo.position = o + radius * concatInfo.normal;
					return true;
				}
				else
				{
					return false;
				}
			}
			if (proj <= axisW + radius)
			{
				//轴上投影处于另一头附近
				var c = (axis * axisW);
				var c2p = p - c;
				if (Vector3.Dot(c2p, c2p) < r2)
				{
					concatInfo.normal = c2p.normalized;
					concatInfo.position = o + c + radius * concatInfo.normal;
					return true;
				}
				else
				{
					return false;
				}
			}
			return false;
		}


		public void EnableCollisionConstraint(ClothPoint[] clothPoints, int index, ref ConcatInfo concatInfo, ref RigidbodyDesc rigidbody)
		{
			//Debug.Log(concatInfo.normal);
			var m1 = rigidbody.mass;

			if (m1 > 0)
			{
				var bounciness = rigidbody.bounciness;
				var m0 = clothPoints[index].weight;
				var dt = Time.deltaTime;
				var v0 = (clothPoints[index].position - clothPoints[index].prePosition) / dt;
				var v0Normal = Vector3.Dot(v0, concatInfo.normal) * concatInfo.normal;
				var v1Normal = Vector3.Dot(rigidbody.velocity, concatInfo.normal) * concatInfo.normal;
				var v0NormalNew = (bounciness + 1) * m1 * v1Normal + v0Normal * (m0 - bounciness * m1);///冲量守恒
				var v1NormalNew = (bounciness + 1) * m0 * v0Normal + v1Normal * (m1 - bounciness * m0);///冲量守恒

				v0NormalNew /= (m0 + m1);
				v1NormalNew /= (m0 + m1);



				rigidbody.velocity = v1NormalNew - v1Normal;
			}

		}



		public void detectingCapsuleCollision(CapsuleCollider capsuleCollider, ClothPoint[] clothPoints, ref RigidbodyDesc capsule, ref Rigidbody capsulerb)
		{

			for (int i = 0; i < clothPoints.Length; i++)
			{
				ConcatInfo concatInfo;
				// 检查布料的每一个顶点是否在AABB内
				if (GetClosestSurfacePoint(clothPoints[i].position, capsuleCollider, out concatInfo))
				{

					// 更新布料的顶点
					clothPoints[i].position = concatInfo.position;/// 发生了碰撞，所以将点移动到胶囊的表面
					RigidbodyDesc tmp = capsule;
					///计算碰撞体速度
					EnableCollisionConstraint(clothPoints, i, ref concatInfo, ref tmp);
					capsule = tmp;
					///对碰撞体施加力
					capsulerb.AddForce(capsule.velocity, ForceMode.VelocityChange);
					//Debug.Log(box.velocity);
				}
			}
		}

		void GetCapsuleEndPoints(CapsuleCollider capsule, out Vector3 point1, out Vector3 point2)
		{
			float halfHeight = capsule.height / 2.0f - capsule.radius;  // 除去半球的高度，得到圆柱体部分的一半高度
			Vector3 offset = capsule.transform.up * halfHeight;  // 圆柱体端点相对于中心的偏移量

			// 将CapsuleCollider的中心位置从本地坐标系转换到世界坐标系
			Vector3 worldCenter = capsule.transform.TransformPoint(capsule.center);

			// 获取两个端点的位置
			point1 = worldCenter + offset;
			point2 = worldCenter - offset;
		}

		bool PointInCylinder(Vector3 point, Vector3 point1, Vector3 point2, float radius)
		{
			Vector3 d = point2 - point1;
			Vector3 pd = point - point1;

			// 点到线段point1-point2的垂直距离
			float distance = Vector3.Cross(pd, d).magnitude / d.magnitude;

			// 检查点是否在圆柱体范围内
			return distance <= radius && Vector3.Dot(pd, d) >= 0 && pd.sqrMagnitude <= d.sqrMagnitude;
		}

		private void HandleCapsuleCollision(ClothPoint point, CapsuleCollider capsule)
		{
			float epsilon = 0.0000001f;
			// 碰撞响应：将点移出胶囊体并给予反弹
			Vector3 direction = (point.position - capsule.transform.position).normalized;
			float distanceToMoveOut = capsule.radius - Vector3.Distance(point.position, capsule.transform.position);
			Vector3 newPointPosition = point.position + direction * (distanceToMoveOut + epsilon);

			// 更新布料顶点位置
			point.position = newPointPosition;

			// 添加反弹力（假设我们有速度和质量的信息）
			//Vector3 velocity = GetVelocity(index); // 假设这个函数可以获取顶点的速度
			//float mass = GetMass(index); // 假设这个函数可以获取顶点的质量
			//float restitution = 0.5f; // 反弹系数，可以根据需要调整
			//Vector3 impulse = mass * restitution * -velocity;
			//ApplyImpulse(index, impulse); // 假设这个函数可以对顶点施加冲量
		}
	}

}