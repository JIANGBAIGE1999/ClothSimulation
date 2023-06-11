using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClothStruct;
using System.Linq;

namespace Hash
{
    public class SpaticalHash
    {
        /// <summary>
        /// 使用空间哈希表防止布料自相交
        /// </summary>
        /// <param name="xi"></param>
        /// <param name="yi"></param>
        /// <param name="zi"></param>
        /// <returns></returns>
        /// 
       
       
        
        
        
        


        private int HashCoords(int xi, int yi, int zi, int tableSize)
        {
            int hash = (xi * 92837111) ^ (yi * 689287499) ^ (zi * 283923481);
            
            return (int)Mathf.Abs(hash) % tableSize; 
        }

        private int IntCoord(float pos, float spacing)
        {
            return (int)Mathf.Floor(pos / spacing);
        }

        /// <summary>
        /// 计算布料顶点在空间网格中的坐标
        /// </summary>
        /// <param name="p">布料顶点坐标</param>
        /// <param name="spacing">网格单位大小</param>
        /// <returns></returns>
        Vector3 squareCoordinates(Vector3 p, float spacing)
        {
            Vector3 result = new Vector3(IntCoord(p.x, spacing), IntCoord(p.y, spacing), IntCoord(p.z, spacing));
            return result;
        }
        /// <summary>
        /// 将空间网格中的坐标映射到哈希表中
        /// </summary>
        private int HashPos(ClothPoint clothPoint, float spacing, int tableSize)
        {
            
            return HashCoords(IntCoord(clothPoint.position.x, spacing), IntCoord(clothPoint.position.y, spacing), IntCoord(clothPoint.position.z, spacing), tableSize);
        }

        

        public int[] GetAdjacentParticles(int id, int[] firstAdjId, ref int[] adjIds)
        {
            int start = firstAdjId[id];
            int end = firstAdjId[id + 1];

            return adjIds.Skip(start).Take(end - start).ToArray();
        }

        public void CreateHash(ClothPoint[] clothPoints,ref int[] cellCount,ref int[] particleMap, int tableSize, float spacing)
        {
            var numObjects = Mathf.Min(clothPoints.Length, particleMap.Length);
            
            ///初始化cellCount和particleMap数组
            for (int i = 0;i < cellCount.Length; i++)//cellCount存储在particleMap中开始寻找的下标。
            {
                cellCount[i] = 0;
            }
            
            for (int i = 0; i < particleMap.Length; i++)
            {
                particleMap[i] = 0;//particleMap是粒子查询数组
            }
            ///将顶点位置进行哈希存入cellCount中
            
            for (int i = 0; i < numObjects; i++)
            {
                int h = HashPos(clothPoints[i], spacing, tableSize);
                cellCount[h]++;
            }
            
            ///重写cellCount数组以包含当前索引前所有元素数量的总和
            int start = 0;
            for (int i = 0; i < tableSize; i++)
            {
                start += cellCount[i];
                cellCount[i] = start;
            }
            cellCount[tableSize] = start;

            ///填充粒子数组
            for (int i = 0; i < numObjects; i++)
            {
                int h = HashPos(clothPoints[i], spacing, tableSize);
                cellCount[h]--;
                particleMap[cellCount[h]] = i;
                
            }

        }

        ///查询哈希表中的项目
        ///queryIds - 被查询到的粒子ID
        ///querySize - 被查询到的粒子总数量
        public void Query(ClothPoint[] clothPoints, int nr, float maxDist,ref int[] cellCount,ref int[] queryIds,ref int querySize, ref int[] particleMap, float spacing, int tableSize)
        {
            int x0 = IntCoord(clothPoints[nr].position.x - maxDist, spacing);
            int y0 = IntCoord(clothPoints[nr].position.y - maxDist, spacing);
            int z0 = IntCoord(clothPoints[nr].position.z - maxDist, spacing);

            int x1 = IntCoord(clothPoints[nr].position.x + maxDist, spacing);
            int y1 = IntCoord(clothPoints[nr].position.y + maxDist, spacing);
            int z1 = IntCoord(clothPoints[nr].position.z + maxDist, spacing);

            querySize = 0;

            for (int xi = x0; xi <= x1; xi++)
            {
                for (int yi = y0; yi <= y1; yi++)
                {
                    for (int zi = z0; zi <= z1; zi++)
                    {
                        int h = HashCoords(xi, yi, zi, tableSize);

                        int start = cellCount[h];
                        int end = cellCount[h + 1];

                        for (int i = start; i < end; i++)
                        {
                            queryIds[querySize] = particleMap[i];
                            ///Debug.Log(queryIds[querySize]);
                            querySize++;
                        }
                    }
                }
            }
        }

        ///queryAll负责循环所有的粒子，为每个粒子调用query()，并进行存储和更新
        public void QueryAll(ClothPoint[] clothPoints, float maxDist, int maxNumObjects,ref int[] adjIds, ref int[] firstAdjId, ref int[] cellCount , ref int[] queryIds, ref int querySize, ref int[] particleMap, float spacing,int tableSize)
        {
            //通过adjIds中//索引所有相邻的id
            int idx = 0;

            // 计算maxDist2的平方，与dist2比较
            float maxDist2 = maxDist * maxDist;

            for (int i = 0; i < maxNumObjects; i++)
            {
                int id0 = i;
                firstAdjId[id0] = idx;//firstAdjId是追踪首次索引到adjIds的位置

                // 查询与此粒子相距maxDist以内的所有粒子
                Query(clothPoints, id0, maxDist, ref cellCount,ref queryIds, ref querySize, ref particleMap, spacing, tableSize);

                // 如果发现粒子，存入adjIds中
                for (int j = 0; j < querySize; j++)
                {
                    int id1 = queryIds[j];

                    // 如果id1>id0就跳过
                    // 跳过id1 == id0
                    if (id1 >= id0) continue;

                    // 计算两个粒子之间的距离平方
                    float dist2 = (clothPoints[id0].position - clothPoints[id1].position).magnitude * (clothPoints[id0].position - clothPoints[id1].position).magnitude;

                    if (dist2 > maxDist2) continue;

                    adjIds[idx++] = id1;//adjIds是与某一特定id相邻的所有顶点id打包成一个密集的数组。
                }
            }

            // 使用当前的idx设置最后一个额外空间
            firstAdjId[maxNumObjects] = idx;
        }
    }
}

