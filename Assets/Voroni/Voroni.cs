using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine;

public class Voroni : MonoBehaviour
{
	public ComputeShader ComputeShader;
	private int m_KernelId;
	public RenderTexture Result;
	public Material VoroniOutput;

	public int TransformsCount = 8;
	private List<Transform> m_Transforms;

	public List<Vector4> Positions;
	public List<Vector4> Colors;

	public bool ByDistance = false;
	public float ScaleFactor = 1;

	void Start()
	{
		if (ByDistance)
		{
			m_KernelId = ComputeShader.FindKernel("VoroniCellDistances");
		}
		else
		{
			m_KernelId = ComputeShader.FindKernel("VoronoiCells");
		}
		Result = new RenderTexture(512, 512, 24);
		Result.filterMode = FilterMode.Point;
		Result.enableRandomWrite = true;
		Result.Create();

		ComputeShader.SetTexture(m_KernelId, "Result", Result);

		m_Transforms = new List<Transform>();
		for (int i = 0; i < TransformsCount; i++)
		{
			m_Transforms.Add(new GameObject(i.ToString()).transform);
			m_Transforms[i].position = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0);
		}

		// Create a random set of colors, 1 for each input transform
		Colors = new List<Vector4>();
		for (int i = 0; i < m_Transforms.Count + 4; i++)
		{
			Colors.Add(new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1));
		}

		ComputeShader.SetVectorArray("Colors", Colors.ToArray());

		VoroniOutput.SetTexture("_MainTex", Result);

		Positions = new List<Vector4>();
	}

	void Update()
	{
		ComputeShader.SetFloat("ScaleFactor", ScaleFactor);

		Positions.Clear();
		for (int i = 0; i < m_Transforms.Count; i++)
		{
			Positions.Add(new Vector4(m_Transforms[i].position.x, m_Transforms[i].position.y, 0, 0));
		}
		Positions.Add(new Vector4(0, 0, 0, 0));
		Positions.Add(new Vector4(0, 1, 0, 0));
		Positions.Add(new Vector4(1, 0, 0, 0));
		Positions.Add(new Vector4(1, 1, 0, 0));
		ComputeShader.SetVectorArray("VoroniInputs", Positions.ToArray());

		ComputeShader.Dispatch(m_KernelId, 512 / 8, 512 / 8, 1);
	}
}
