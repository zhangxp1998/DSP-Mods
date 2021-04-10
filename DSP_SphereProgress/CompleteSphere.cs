using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSP_Mods.SphereProgress
{
    class CompleteSphere
    {
        public static void CompleteLayer(DysonSphereLayer layer, float percent = 0.1f)
        {
            foreach (var node in DysonSphereUtils.GetNodes(layer))
            {
                int spRequired = Math.Min((int)Math.Ceiling(node.totalSpMax * percent), node.spReqOrder);
                for (int j = 0; j < spRequired; j++)
                {
                    // layer.dysonSphere.OrderConstructSp(node);
                    layer.dysonSphere.ConstructSp(node);
                }
            }
            foreach (var node in DysonSphereUtils.GetNodes(layer))
            {
                int cpRequired = Math.Min((int)Math.Ceiling(node.totalCpMax * percent), node.cpReqOrder);
                for (int j = 0; j < cpRequired; j++)
                {
                    node.ConstructCp();
                }
            }
        }
    }
}
