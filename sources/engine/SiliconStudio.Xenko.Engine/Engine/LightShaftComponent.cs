// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// The source for light shafts, should be placed on the same entity as the light component which will be used for light shafts
    /// </summary>
    [Display("Light Shaft", Expand = ExpandRule.Always)]
    [DataContract("LightShaftComponent")]
    [DefaultEntityComponentProcessor(typeof(LightShaftProcessor), ExecutionMode = ExecutionMode.All)]
    public class LightShaftComponent : ActivableEntityComponent
    {
        /// <summary>
        /// Density of the light shaft fog
        /// </summary>
        /// <userdoc>
        /// Density of the light shaft fog
        /// </userdoc>
        public float DensityFactor { get; set; } = 0.002f;
        
        /// <summary>
        /// Number of samples taken per pixel
        /// </summary>
        /// <userdoc>
        /// Number of samples taken per pixel
        /// </userdoc>
        public int SampleCount { get; set; } = 16;

        /// <summary>
        /// If true, all bounding volumes will be drawn one by one.
        /// </summary>
        /// <remarks>
        /// If this is off, the light shafts might be lower in quality if the bounding volumes overlap (in the same pixel). 
        /// If this is on, and the bounding volumes overlap (in space), the light shafts inside the overlapping area will become twice as bright.
        /// </remarks>
        public bool SeparateBoundingVolumes { get; set; } = true;
    }
}