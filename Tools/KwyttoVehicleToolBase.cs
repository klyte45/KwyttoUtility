using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace Kwytto.Tools
{
    public class KwyttoVehicleToolBase : KwyttoToolBase
    {
        protected ushort m_hoverVehicle;
        protected ushort m_hoverParkedVehicle;
        protected bool m_trailersAlso;
        protected static Color m_hoverColor = new Color32(47, byte.MaxValue, 47, byte.MaxValue);
        protected static Color m_removeColor = new Color32(byte.MaxValue, 47, 47, 191);
        protected static Color m_despawnColor = new Color32(byte.MaxValue, 160, 47, 191);
        protected override void Awake()
        {
            base.Awake();
            enabled = false;
        }
        protected override void OnRaycastHoverInstance(Ray mouseRay)
        {
            m_hoverVehicle = 0;
            m_hoverParkedVehicle = 0;
            RaycastHoverInstance(mouseRay);
        }
        protected static ref Vehicle[] VehicleBuffer => ref Singleton<VehicleManager>.instance.m_vehicles.m_buffer;
        protected static ref VehicleParked[] VehicleParkedBuffer => ref Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer;

        private void RaycastHoverInstance(Ray mouseRay)
        {

            var input = new ToolBase.RaycastInput(mouseRay, Camera.main.farClipPlane);
            Vector3 origin = input.m_ray.origin;
            Vector3 normalized = input.m_ray.direction.normalized;
            Vector3 vector = input.m_ray.origin + (normalized * input.m_length);
            var ray = new Segment3(origin, vector);

            VehicleManager.instance.RayCast(ray, 0, 0, out _, out m_hoverVehicle, out m_hoverParkedVehicle);
        }
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (m_hoverVehicle != 0)
            {
                m_trailersAlso = false;
                Color toolColor = m_hoverColor;
                RenderOverlay(cameraInfo, toolColor, m_hoverVehicle, false);
                return;
            }
            if (m_hoverParkedVehicle != 0)
            {
                m_trailersAlso = false;
                Color toolColor = m_hoverColor;
                RenderOverlay(cameraInfo, toolColor, m_hoverParkedVehicle, true);
                return;
            }
        }
        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color toolColor, ushort vehicleId, bool parked)
        {
            if (vehicleId == 0)
            {
                return;
            }
            if (m_trailersAlso && !parked)
            {
                var subVehicle = VehicleBuffer[vehicleId].GetFirstVehicle(vehicleId);
                while (subVehicle > 0)
                {
                    HoverVehicle(cameraInfo, toolColor, subVehicle);
                    subVehicle = VehicleBuffer[subVehicle].m_trailingVehicle;
                }
            }
            else
            {
                if (parked)
                {
                    HoverParkedVehicle(cameraInfo, toolColor, vehicleId);
                }
                else
                {
                    HoverVehicle(cameraInfo, toolColor, vehicleId);
                }
            }
        }
        private static void HoverVehicle(RenderManager.CameraInfo cameraInfo, Color toolColor, ushort vehicleId) => VehicleBuffer[vehicleId].RenderOverlay(cameraInfo, vehicleId, toolColor);
        private static void HoverParkedVehicle(RenderManager.CameraInfo cameraInfo, Color toolColor, ushort vehicleId) => VehicleParkedBuffer[vehicleId].RenderOverlay(cameraInfo, vehicleId, toolColor);
    }
}