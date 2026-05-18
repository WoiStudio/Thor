using System;
using System.Collections.Generic;
using sc.splinemesher.pro.runtime;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace sc.splinemesher.pro.editor
{
    public static partial class SplineMesherEditor
    {
        private static void OnBakeryStart(object sender, EventArgs e)
        {
            OnLightBakeStart();
        }
        
        private static void OnLightBakeStart()
        {
            if (SplineMesherSettings.GenerateLightmapUV == SplineMesherSettings.BakingMode.Automatic)
            {
                #if SM_DEV
                Debug.Log("[Spline Mesher] Lightbake started. Automatically generating lightmap UV's if needed");
                #endif
                
                GenerateLightmapUVs();
            }
        }
        
        /// <summary>
        /// Generates lightmap UV's for any spline mesh that requires it
        /// </summary>
        public static void GenerateLightmapUVs()
        {
            #if UNITY_6000_4_OR_NEWER
            SplineCurveMesher[] splineMeshers = Object.FindObjectsByType<SplineCurveMesher>();
            #else
            SplineCurveMesher[] splineMeshers = Object.FindObjectsByType<SplineCurveMesher>(FindObjectsSortMode.None);
            #endif
            
            int count = 0;
            System.Diagnostics.Stopwatch lightmapUVUnwrapTimer = new System.Diagnostics.Stopwatch();
            
            lightmapUVUnwrapTimer.Start();
            //Find the spline meshes that still require lightmap UV's
            foreach (SplineCurveMesher splineMesher in splineMeshers)
            {
                if (GenerateLightmapUV(splineMesher))
                {
                    count++;
                }
            }
            lightmapUVUnwrapTimer.Stop();
            
            if (count > 0)
            {
                Debug.Log($"[Spline Mesher] Lightmap UV created for {count} spline meshes (Duration: {lightmapUVUnwrapTimer.ElapsedMilliseconds}ms)");
            }
        }

        public static bool GenerateLightmapUV(SplineCurveMesher splineMesher)
        {
            bool generated = false;
            for (int i = 0; i < splineMesher.Containers.Count; i++)
            {
                SplineMeshContainer container = splineMesher.Containers[i];

                for (int j = 0; j < container.SegmentCount; j++)
                {
                    SplineMeshSegment segment = container.Segments[j];

                    if (RequiresLightmapUV(segment))
                    {
                        #if SM_DEV
                        //Debug.Log($"{splineMesher.name} segment #{j} requires new lightmap UV's");
                        #endif

                        GenerateLightmapUV(segment, splineMesher.settings.output.lightmapUVAngleThreshold, splineMesher.settings.output.lightmapUVMarginMultiplier);

                        generated = true;
                    }
                }
            }

            return generated;
        }

        public static bool RequiresLightmapUV(SplineCurveMesher splineMesher)
        {
            for (int i = 0; i < splineMesher.Containers.Count; i++)
            {
                SplineMeshContainer container = splineMesher.Containers[i];

                for (int j = 0; j < container.SegmentCount; j++)
                {
                    SplineMeshSegment segment = container.Segments[j];

                    if (RequiresLightmapUV(segment))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        private static bool RequiresLightmapUV(SplineMeshSegment segment)
        {
            Mesh mesh = segment.mesh;

            if (mesh == null) return false;
            
            StaticEditorFlags staticFlags = GameObjectUtility.GetStaticEditorFlags(segment.gameObject);

            if (staticFlags.HasFlag(StaticEditorFlags.ContributeGI) == false) return false;
            
            if (mesh.HasVertexAttribute(VertexAttribute.TexCoord1))
            {
                return false;
            }

            /*
            //Conditions that indicate the mesh has no lightmap UV's
            //Note that rebuilding the spline mesh clears the UV2 channel, automatically marking it as 'dirty' again.
            if (mesh.uv2 == null || mesh.uv2.Length == 0)
            {
                return true;
            }

            */
            
            return true;
        }
        
        public static void GenerateLightmapUV(SplineMeshSegment segment, float angleThreshold, float marginMultiplier)
        {
            MeshFilter mf = segment.GetComponent<MeshFilter>();

            if (!mf) return;
            
            Mesh originalMesh = mf.sharedMesh;
            if(originalMesh == null) return;
            
            //Unity's lightmap UV generation does not accept meshes with 8-16bit vertex data, causing the editor to crash
            //A workaround is to create a copy with vertex data converted back to 32bit
            Mesh mesh32Bit = Create32BitCopy(originalMesh);

            UnwrapParam.SetDefaults(out var unwrapSettings);
            unwrapSettings.hardAngle = angleThreshold;
            unwrapSettings.packMargin *= Mathf.Max(0.01f, marginMultiplier);

            //Creates a lightmap UV, through largely unknown methods
            //Will split vertices along UV seams, resulting in a higher vertex count
            if (Unwrapping.GenerateSecondaryUVSet(mesh32Bit, unwrapSettings) == false)
            {
                throw new Exception($"Lightmap UV generation failed for {originalMesh.name}");
            }
            
            mf.sharedMesh = mesh32Bit;
        }
        
        private static Mesh Create32BitCopy(Mesh originalMesh)
        {
            Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(originalMesh);
            Mesh.MeshData sourceData = meshDataArray[0];

            NativeArray<CurveToMesh.Vertex> sourceVertices = sourceData.GetVertexData<CurveToMesh.Vertex>();
            int vertexCount = sourceData.vertexCount;
            var sourceIndexData = sourceData.GetIndexData<ushort>();
            int indexCount = sourceIndexData.Length;
            
            NativeArray<CurveToMesh.Vertex32Bit> vertices = new NativeArray<CurveToMesh.Vertex32Bit>(vertexCount, Allocator.Temp);
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = new CurveToMesh.Vertex32Bit(sourceVertices[i]);
            }
            
            MeshUpdateFlags noValidation = MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds;
            
            Mesh tempMesh = new Mesh();
            tempMesh.name = originalMesh.name;
            
            tempMesh.SetVertexBufferParams(vertexCount, CurveToMesh.VertexAttributes32Bit);

            tempMesh.SetVertexBufferData(vertices, 0, 0, vertexCount, 0, noValidation);
            sourceVertices.Dispose();
            vertices.Dispose();
            
            tempMesh.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
            tempMesh.SetIndexBufferData(sourceIndexData, 0, 0, indexCount, noValidation);
            sourceIndexData.Dispose();
            
            tempMesh.subMeshCount = sourceData.subMeshCount;
            for (int i = 0; i < tempMesh.subMeshCount; i++)
            {
                tempMesh.SetSubMesh(i, sourceData.GetSubMesh(i), noValidation);
            }
            
            tempMesh.bounds = originalMesh.bounds;
            
            Utilities.Destroy(originalMesh);
            
            meshDataArray.Dispose();

            return tempMesh;
        }
    }
}