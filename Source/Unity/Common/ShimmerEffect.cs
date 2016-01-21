//--------------------------------------------------------------------------------
// Copyright (c) North Carolina State University
//
// Department of Computer Science 
// The IntelliMedia Group
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//--------------------------------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class allows a shimmer effect to be applied to objects. The shimmer effect can
/// be turned off by setting IsAnimating to false.
/// </summary>
using IntelliMedia;


public class ShimmerEffect : MonoBehaviour
{	
    private const string ShimmerShaderName = "IntelliMedia/Shimmer Bumped Diffuse";

	public bool IsAnimating
	{
		get
		{
			return this.isAnimating;
		}

		set
		{	
			this.isAnimating = value;

            if (this.isAnimating)
            {
                GetComponent<Renderer>().material.shader = shimmerShader;
				if(this.name.StartsWith("Food"))
				{
					GetComponent<Renderer>().material.SetColor("_ShimmerTint",new Vector4(14/255f,142/255f,140/255f,1f));
				}
            }
            else
            {
                GetComponent<Renderer>().material.shader = originalShader;
            }
		}
	}

    private bool isAnimating;
    private Shader originalShader = null;
    private static Shader shimmerShader = null;

    void Awake()
    {
        this.originalShader = GetComponent<Renderer>().material.shader;

		if (originalShader == shimmerShader) 
		{
			this.originalShader = Shader.Find ("Diffuse");
		}
        
		if (shimmerShader == null)
        {
            shimmerShader = Shader.Find(ShimmerShaderName);
            if (shimmerShader == null)
            {
                DebugLog.Error("Unabled to find shader: {0}", ShimmerShaderName);
            }
        }

		this.IsAnimating = true;
    }
}
