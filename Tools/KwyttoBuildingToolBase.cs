using ColossalFramework;
using ColossalFramework.Math;
using Kwytto.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Kwytto.Tools
{
    public class KwyttoBuildingToolBase : KwyttoToolBase
    {
        protected ushort m_hoverBuilding;
        public bool m_parentOnly = true;

        public static readonly Color m_hoverColor = new Color32(47, byte.MaxValue, 47, byte.MaxValue);
        public static readonly Color m_removeColor = new Color32(byte.MaxValue, 47, 47, 191);
        public static readonly Color m_despawnColor = new Color32(byte.MaxValue, 160, 47, 191);
        protected override void Awake()
        {
            base.Awake();
            enabled = false;
        }
        protected override void OnRaycastHoverInstance(Ray mouseRay)
        {
            m_hoverBuilding = 0;
            RaycastHoverInstance(mouseRay);
        }
        protected static Building[] BuildingBuffer => Singleton<BuildingManager>.instance.m_buildings.m_buffer;

        private void RaycastHoverInstance(Ray mouseRay)
        {
            var input = new ToolBase.RaycastInput(mouseRay, Camera.main.farClipPlane);
            Vector3 origin = input.m_ray.origin;
            Vector3 normalized = input.m_ray.direction.normalized;
            Vector3 vector = input.m_ray.origin + (normalized * input.m_length);
            var ray = new Segment3(origin, vector);

            BuildingManager.instance.RayCast(ray, ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default, Building.Flags.None, out _, out m_hoverBuilding);
            if (m_parentOnly)
            {
                var i = 0;
                while (BuildingBuffer[m_hoverBuilding].m_parentBuilding != 0 && i < 10)
                {
                    m_hoverBuilding = BuildingBuffer[m_hoverBuilding].m_parentBuilding;
                    i++;
                }
            }

        }
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            Color toolColor = m_hoverColor;
            RenderOverlay(cameraInfo, toolColor, m_hoverBuilding);
        }

        public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color toolColor, ushort buildingId)
        {
            if (buildingId == 0)
            {
                return;
            }
            HoverBuilding(cameraInfo, toolColor, buildingId);
            if (m_parentOnly)
            {
                var subBuilding = BuildingBuffer[buildingId].m_subBuilding;
                while (subBuilding > 0)
                {
                    HoverBuilding(cameraInfo, toolColor, subBuilding);
                    subBuilding = BuildingBuffer[subBuilding].m_subBuilding;
                }
            }
        }
        private static void HoverBuilding(RenderManager.CameraInfo cameraInfo, Color toolColor, ushort buildingId)
        {
            BuildingBuffer[buildingId].GetTotalPosition(out Vector3 pos, out Quaternion rot, out Vector3 size);
            var quad = new Quad3(
               (rot * new Vector3(-size.x, 0, size.z) / 2) + pos,
               (rot * new Vector3(-size.x, 0, -size.z) / 2) + pos,
               (rot * new Vector3(size.x, 0, -size.z) / 2) + pos,
               (rot * new Vector3(size.x, 0, size.z) / 2) + pos
            );
            Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, toolColor, quad, -1f, 1280f, false, true);

            var nets = BuildingBuffer[buildingId].m_netNode;
            var drawnSegments = new HashSet<ushort>();
            var netManagerInstance = NetManager.instance;
            while (nets > 0)
            {
                ref NetNode currNode = ref netManagerInstance.m_nodes.m_buffer[nets];
                for (int i = 0; i < 8; i++)
                {
                    var segmentId = currNode.GetSegment(i);
                    if (segmentId == 0)
                    {
                        break;
                    }
                    if (drawnSegments.Contains(segmentId))
                    {
                        continue;
                    }
                    ref NetSegment nextSegment = ref netManagerInstance.m_segments.m_buffer[segmentId];
                    if (netManagerInstance.m_nodes.m_buffer[nextSegment.GetOtherNode(nets)].m_building != buildingId)
                    {
                        continue;
                    }
                    drawnSegments.Add(segmentId);
                    RenderOverlayUtils.RenderNetSegmentOverlay(cameraInfo, toolColor, segmentId);
                }
                nets = currNode.m_nextBuildingNode;
            }
        }
    }
}