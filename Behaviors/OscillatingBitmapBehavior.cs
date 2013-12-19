using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// # OscillatingBitmapBehavior
// A bitmap "image" made up of GameObjects, which may optionally be oscillated.
//
// It reads a text file, where "X" indicates a "pixel" to be drawn, e.g.:
//
// X  X
//
// XXXX
//
// Anything besides "X" will be ignored, so the following would produce the same image as above:
//
// XOOX
// OOOO
// XXXX
//
// Have fun!
public class OscillatingBitmapBehavior : MonoBehaviour {
	private List<GameObject> parts;
	private List<Vector3> destinations;
	private List<Vector3> oscillationVectors;
	private List<float> oscillationTimes;
	private List<float> oscillationSpeeds;
	private Vector3 finalExtents;
	private Vector3 extents;

	// ## tightness
	// How tightly we want the parts to following the calculated oscillation
	public float tightness = 1f;

	// ## partSize
	// The width/height of the individual bitmap part ("pixel")
	public float partSize = 0.5f;

	// ## partName
	// Name of the part prefab to be instatiated
	public string partName = "EnemyCube";

	// ## bitmapName
	// Name of the text file that contains the bitmap data
	public string bitmapName = "seeker-bitmap.txt";

	// ## bitmapRoot
	// Root folder under Application.streamingAssetsPath that contains the bitmap text files.
	private string bitmapRoot = "Enemies";

	// ## bitmap
	// Array of string data representing image data
	private string[] bitmap;

	// Use this for initialization
	void Start ()
	{
		LoadBitmap();
	}

	void Update()
	{
		UpdateParts();
	}

	// ## UpdateParts
	// Updates the part positions.
	private void UpdateParts()
	{
		if (parts == null) {
			return;
		}

		Vector3 offset = new Vector3(
			Mathf.Max(finalExtents.x - partSize/2f, 0f),
			Mathf.Max(finalExtents.y + partSize/2f, 0f),
			0f
		);

		for (int i = 0; i < parts.Count; i++) {
			if (i < destinations.Count) {
				Vector3 destination = transform.position - offset + destinations[i] +
					oscillationVectors[i] * Mathf.Sin(oscillationTimes[i]);

				parts[i].transform.position = Vector3.Lerp(
					parts[i].transform.position,
					destination,
					Time.deltaTime * tightness
				);

				// update oscillation times
				oscillationTimes[i] += oscillationSpeeds[i];
			}
		}
	}

	// ## LoadBitmap
	// Loads the bitmap text file and generates parts for each "pixel" to be rendered.
	public void LoadBitmap()
	{
		bitmap = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, string.Format("{0}/{1}", bitmapRoot, bitmapName)));

		if (parts == null) {
			parts = new List<GameObject>();
		}

		if (bitmap == null) {
			Debug.LogWarning("No bitmap assigned.");
			return;
		}

		destinations = new List<Vector3>();
		oscillationVectors = new List<Vector3>();
		oscillationSpeeds = new List<float>();
		oscillationTimes = new List<float>();

		// read the string array
		// from BOTTOM up
		int height = bitmap.Length;
		for (int y = bitmap.Length - 1; y >= 0; y--) {
			for (int x = 0; x < bitmap[y].Length; x++) {
				if (bitmap[y][x] == 'X') {
					destinations.Add(
						new Vector3(
							x * partSize,
							(height - y) * partSize,
							0f
						)
					);
				}
			}
		}

		finalExtents = CalculateExtentsFinal();
	}

	// ## ClearParts
	// Clears local references to our constituent parts.
	// This is useful when we want to manually animate them on destruction, for example.
	public void ClearParts()
	{
		parts = null;
	}

	// ## Parts
	// The list of parts that make up this bitmap.
	public List<GameObject> Parts
	{
		get { return parts; } 
	}

	// ## AddPart
	// Adds another part to the image, returning false if it was unable to be added;
	// this is normally due to the fact that all bits have been added already.
	//
	// Parameters:
	//
	// - oscillationVector: vector along which you want the part to oscillate
	// - t: where in the oscillation cycle you want to start at (using sine function)
	public bool AddPart(Vector3 oscillationVector, float oscillationSpeed, float t)
	{
		if (parts.Count < destinations.Count)
		{
			GameObject instance = (GameObject)Instantiate(
				(GameObject)Resources.Load(partName),
				transform.position,
				Quaternion.identity
			);
			instance.name = "Enemy-" + instance.name;

			parts.Add(instance);
			oscillationVectors.Add(oscillationVector);
			oscillationSpeeds.Add(oscillationSpeed);
			oscillationTimes.Add(t);

			// update extents
			extents = CalculateExtentsCurrent();

			return true;
		}
		else
		{
			return false;
		}
	}

	// ## RemovePartAt
	public bool RemovePartAt(int pos)
	{
		if (parts != null && parts.Count > pos)
		{
			parts.RemoveAt(pos);
			return true;
		}

		return false;
	}

	// ## CalculateExtentsCurrent
	// Calculates the current total extents of the enemy, accounting for the volume of the cubes
	public Vector3 CalculateExtentsCurrent()
	{
		float xMax = 0f;
		float yMax = 0f;
		float xMin = Mathf.Infinity;
		float yMin = Mathf.Infinity;

		for (int i = 0; i < parts.Count; i++)
		{
			Vector3 d = destinations[i];

			if (d.x > xMax)
			{
				xMax = d.x;
			}
			if (d.y > yMax)
			{
				yMax = d.y;
			}
			if (d.x < xMin)
			{
				xMin = d.x;
			}
			if (d.y < yMin)
			{
				yMin = d.y;
			}
		}

		return new Vector3((xMax - xMin + partSize) / 2f, (yMax - yMin + partSize) / 2f, 1f);
	}

	// ## CalculateExtentsFinal
	// Calculates what the final extents will be when the bitmap is complete.
	public Vector3 CalculateExtentsFinal()
	{
		float xMax = 0f;
		float yMax = 0f;
		float xMin = Mathf.Infinity;
		float yMin = Mathf.Infinity;

		foreach (Vector3 d in destinations)
		{
			if (d.x > xMax)
			{
				xMax = d.x;
			}
			if (d.y > yMax)
			{
				yMax = d.y;
			}
			if (d.x < xMin)
			{
				xMin = d.x;
			}
			if (d.y < yMin)
			{
				yMin = d.y;
			}
		}

		return new Vector3((xMax - xMin + partSize) / 2f, (yMax - yMin + partSize) / 2f, 1f);
	}

	void OnDestroy()
	{
		if (parts != null)
		{
			foreach (GameObject p in parts)
			{
				// TODO: cool graphical effect
				Destroy(p);
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (destinations != null)
		{
			Gizmos.DrawWireCube(transform.position, finalExtents * 2f);
		}
	}
}
