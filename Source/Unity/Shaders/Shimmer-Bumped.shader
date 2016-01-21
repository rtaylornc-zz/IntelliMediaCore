//-------------------------------------------------------------------------------------------------
// Copyright (c) Bradford W. Mott
// IntelliMedia, Center for Educational Informatics, North Carolina State University
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
// SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
// OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//-------------------------------------------------------------------------------------------------

// This shader renders a shimmer effect to highlight important items in the game world. The
// effect is similar to ones used in Bioshock and Resident Evil and is loosely based on an
// fx shader developed by Mark Blosser. 
Shader "IntelliMedia/Shimmer Bumped Diffuse" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_ShimmerSpeed ("Shimmer Speed", Float) = 3.0
		_ShimmerExponent ("Shimmer Exponent", Float) = 4.0
		_ShimmerTint ("Shimmer Tint", Color) = (0.75, 0.75, 0.75, 1.0)
		_RotationAxis ("Rotation Axis", Vector) = (1.0, 1.0, 1.0, 0.0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 300
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma target 2.0
		
		fixed4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		float _ShimmerSpeed;
		float _ShimmerExponent;
		float4 _ShimmerTint;
		float4 _RotationAxis;
		
		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 wNorm;
			float3 rotPerpVec;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.wNorm = mul((float3x3)_Object2World, v.normal);
			
			float angle = _Time.y * _ShimmerSpeed;
			
			// Compute matrix to perform a rotation around the specified rotation axis
    		float3 u = _RotationAxis.xyz;
    		float c = cos(angle);
    		float s = sin(angle);
		    float t = 1.0 - c;
    		float3x3 mat = float3x3(
    			t * u.x * u.x + c,        t * u.x * u.y - s * u.z,  t * u.x * u.z + s * u.y,
				t * u.x * u.y + s * u.z,  t * u.y * u.y + c,        t * u.y * u.z - s * u.x,
				t * u.x * u.z - s * u.y,  t * u.y * u.z + s * u.x,  t * u.z * u.z + c);

			// Rotate a vector that's perpendicular to the rotation axis. The prependicular vector
			// calculation will fail with some choices of the rotation axis (e.g., [-1, 1, 0]). This
			// failure case is not tested for to simplify the shader.
            o.rotPerpVec = normalize(mul(mat, float3(u.z, u.z, - u.x - u.y)));
		}
		
		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = (tex2D(_MainTex, IN.uv_MainTex) * _Color).rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Emission = _ShimmerTint * pow(0.5 * dot(IN.rotPerpVec, normalize(IN.wNorm)) + 0.5, _ShimmerExponent);
		}
		ENDCG
	}
	
	FallBack "Diffuse"
}
