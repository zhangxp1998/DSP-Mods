using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DysonSphereSave
{
    class CompleteSphere
    {
        public static void CompleteLayer(DysonSphereLayer layer, float percent = 0.1f)
        {
            /*layer.dysonSphere.starData.luminosity = (float)(2.5 *Math.Pow(36000.0f / 8200.0f, 2));
            layer.dysonSphere.starData.luminosity = (float)Math.Pow(layer.dysonSphere.starData.luminosity, 3);
            var dysonSphere = layer.dysonSphere;
            var dysonLumino = dysonSphere.starData.dysonLumino;
            dysonSphere.energyGenPerSail = Configs.freeMode.solarSailEnergyPerTick;
            dysonSphere.energyGenPerNode = Configs.freeMode.dysonNodeEnergyPerTick;
            dysonSphere.energyGenPerFrame = Configs.freeMode.dysonFrameEnergyPerTick;
            dysonSphere.energyGenPerShell = Configs.freeMode.dysonShellEnergyPerTick;
            dysonSphere.energyGenPerSail = (long)(dysonSphere.energyGenPerSail * dysonLumino);
            dysonSphere.energyGenPerNode = (long)(dysonSphere.energyGenPerNode * dysonLumino);
            dysonSphere.energyGenPerFrame = (long)(dysonSphere.energyGenPerFrame * dysonLumino);
            dysonSphere.energyGenPerShell = (long)(dysonSphere.energyGenPerShell * dysonLumino);
            var swarm = layer.dysonSphere.swarm;*/
            
            foreach (var node in DysonSphereUtils.GetNodes(layer))
            {
                int spRequired = Math.Min((int)Math.Ceiling(node.totalSpMax * percent), node.spReqOrder);
                for (int j = 0; j < spRequired; j++)
                {
                    layer.dysonSphere.ConstructSp(node);
                }
            }
            foreach (var node in DysonSphereUtils.GetNodes(layer))
            {
                long cpRequired = Math.Min((long)Math.Ceiling(node.totalCpMax * percent), node.cpReqOrder);
                for (int j = 0; j < cpRequired; j++)
                {
                    node.ConstructCp();
                }
            }
        }
    }
}
