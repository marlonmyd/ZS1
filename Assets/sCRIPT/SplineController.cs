using UnityEngine;
using System.Collections;

public enum eOrientationMode { NODE = 0, TANGENT }

[AddComponentMenu("Splines/Spline Controller")]
[RequireComponent(typeof(SplineInterpolator))]
public class SplineController : MonoBehaviour
{
	public GameObject SplineRoot;
	public float Duration = 10;
	public eOrientationMode OrientationMode = eOrientationMode.NODE;
	public eWrapMode WrapMode = eWrapMode.ONCE;
	public bool AutoStart = true;
	public bool AutoClose = true;
	public bool HideOnExecute = true;
	public float DistanceRun;
	public bool Go=true;
	SplineInterpolator mSplineInterp;
	Transform[] mTransforms;

	
	void FixedUpdate(){
	if(Go){
	DistanceRun += (Time.deltaTime*Duration)/10;
	}
	}
	void OnDrawGizmos()
	{
		Transform[] trans = GetTransforms();
		if (trans.Length < 2)
			return;

		SplineInterpolator interp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;
		SetupSplineInterpolator(interp, trans);
		interp.StartInterpolation(null, false, WrapMode);


		Vector3 prevPos = trans[0].position;
		for (int c = 1; c <= 100; c++)
		{
			float currTime = c * Duration / 100;
			Vector3 currPos = interp.GetHermiteAtTime(currTime);
			float mag = (currPos-prevPos).magnitude * 2;
			Gizmos.color = new Color(mag, 0, 0, 1);
			Gizmos.DrawLine(prevPos, currPos);
			prevPos = currPos;
		}
	}


	void Start()
	{Duration=Random.Range(30,50);
		mSplineInterp = GetComponent(typeof(SplineInterpolator)) as SplineInterpolator;

		Setup();

		if (AutoStart)
			FollowSpline();
	}

	void Setup()
	{
		mTransforms = GetTransforms();

		if (HideOnExecute)
			DisableTransforms();
	}

	void SetupSplineInterpolator(SplineInterpolator interp, Transform[] trans)
	{
		interp.Reset();
		
		float step = (AutoClose) ? Duration / trans.Length :
			Duration / (trans.Length - 1);

		int c;
		for (c = 0; c < trans.Length; c++)
		{
			if (OrientationMode == eOrientationMode.NODE)
			{
				interp.AddPoint(trans[c].position, trans[c].rotation, step * c, new Vector2(0, 1));
			}
			else if (OrientationMode == eOrientationMode.TANGENT)
			{
				Quaternion rot;
				if (c != trans.Length - 1)
					rot = Quaternion.LookRotation(trans[c + 1].position - trans[c].position, trans[c].up);
				else if (AutoClose)
					rot = Quaternion.LookRotation(trans[0].position - trans[c].position, trans[c].up);
				else
					rot = trans[c].rotation;

				interp.AddPoint(trans[c].position, rot, step * c, new Vector2(0, 1));
			}
		}

		if (AutoClose)
			interp.SetAutoCloseMode(step * c);
	}


	/// <summary>
	/// Returns children transforms, sorted by name.
	/// </summary>
	Transform[] GetTransforms()
	{
		if (SplineRoot != null)
		{
			ArrayList transforms = new ArrayList(SplineRoot.GetComponentsInChildren(typeof(Transform)));
			
			transforms.Remove(SplineRoot.transform);
			
			transforms.Sort(new GameObjectNameComparer());

			return (Transform[])transforms.ToArray(typeof(Transform));
		}

		return null;
	}

	/// <summary>
	/// Disables the spline objects, we don't need them outside design-time.
	/// </summary>
	void DisableTransforms()
	{
		if (SplineRoot != null)
		{
			SplineRoot.SetActiveRecursively(false);
		}
	}


	/// <summary>
	/// Starts the interpolation
	/// </summary>
	void FollowSpline()
	{
		if (mTransforms.Length > 0)
		{
			SetupSplineInterpolator(mSplineInterp, mTransforms);
			mSplineInterp.StartInterpolation(null, true, WrapMode);
			
		}
	}

	/// <summary>
	/// Starts the interpolation and calls the delegate specified
	/// </summary>
	void FollowSpline(OnEndCallback callback)
	{
		if (mTransforms.Length > 0)
		{
			SetupSplineInterpolator(mSplineInterp, mTransforms);
			mSplineInterp.StartInterpolation(callback, true, WrapMode);
		}
	}

	public void Play(OnEndCallback callback)
	{
		Setup();
		FollowSpline(callback);
	}

	public void Play()
	{
		Setup();
		FollowSpline();
	}
	void OnTriggerEnter(Collider other){
	if(other.tag =="lap"){
	
	Duration=Random.Range(50,110);
	}
	}
}


public class GameObjectNameComparer : IComparer
{
	int IComparer.Compare(object a, object b)
	{
		return ((Transform)a).name.CompareTo(((Transform)b).name);
	}
}