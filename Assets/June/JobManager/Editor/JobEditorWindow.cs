using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class JobEditorWindow : EditorWindow {

	public static JobEditorWindow Instance {
		get;
		private set;
	}

	private static Color HIGHLIGHT_COLOUR = Color.red;

	public static List<JobDisplay> JOBS = new List<JobDisplay>();

	private static Dictionary<JobDisplay, Color> _JOB_COLOUR = new Dictionary<JobDisplay, Color>();

	public const float HIGHLIGHT_TIME = 2f;

	[MenuItem("June/Jobs")]
	public static void OpenJobWindow() {
		var window = EditorWindow.GetWindow(typeof(JobEditorWindow));
		Instance = (JobEditorWindow)window;
		window.Show();
	}

	#region Callbacks
	/// <summary>
	/// Raises the enable event.
	/// </summary>
	private void OnEnable() {
		InitCallback();
	}

	/// <summary>
	/// Inits the callback.
	/// </summary>
	private void InitCallback() {
		Debug.Log("[JobEditorWindow] Setting callback");
		Job._OnJobStarted = HandleOnJobStarted;
		Job._OnJobCompleted = HandleOnJobCompleted;
	}

	/// <summary>
	/// Handles the on job started.
	/// </summary>
	/// <param name="job">Job.</param>
	public static void HandleOnJobStarted(Job job) {
		JOBS.Add(
			new JobDisplay(
				job, 
				string.Format("Job #{0} {1}", JOBS.Count+1,JobDisplay.GetJobType(job))));
	}

	/// <summary>
	/// Handles the on job completed.
	/// </summary>
	/// <param name="job">Job.</param>
	public static void HandleOnJobCompleted(Job job) {
		var jobD = JOBS.FirstOrDefault(j => j.Job == job);
		jobD.ShouldRefresh = false;
		if(null != jobD) {
			_JOB_COLOUR.Add(jobD, GUI.contentColor);
		}
	}
	#endregion

	#region Render UI
	Vector2 _ScrollPosition;

	public void Update() {
		// Callbacks
		var keys = _JOB_COLOUR.Keys.ToList();
		foreach(var key in keys) {
			_JOB_COLOUR[key] = Color.Lerp(_JOB_COLOUR[key], HIGHLIGHT_COLOUR, Time.deltaTime * (5f/HIGHLIGHT_TIME));
			
			if(_JOB_COLOUR[key] == HIGHLIGHT_COLOUR) {
				_JOB_COLOUR.Remove(key);
				JOBS.Remove(key);
			}
		}

		Repaint();
	}

	/// <summary>
	/// Handles the GUI event.
	/// </summary>
	void OnGUI() {

		EditorGUILayout.LabelField("Current Jobs", EditorStyles.largeLabel);

		using(var vert = new JuneVerticalSection())
		using(var scroll = new JuneScrollView(ref _ScrollPosition)) {
			RenderJobs(JOBS);
		}
	}


	/// <summary>
	/// Renders the jobs.
	/// </summary>
	/// <param name="jobs">Jobs.</param>
	private void RenderJobs(List<JobDisplay> jobs) {
		using(var vert = new JuneVerticalSection()) {
			for(int i=0; i<jobs.Count; i++) {
				RenderJob(jobs[i]);
			}
		}
	}

	/// <summary>
	/// Renders the job.
	/// </summary>
	/// <param name="job">Job.</param>
	private void RenderJob(JobDisplay job) {
		if(null != job) {
			job.Refresh();
		
			using(var jobSection = new JuneHorizontalSection()) {

				using(var labelSection = new JuneVerticalSection()) {
					GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
					if(_JOB_COLOUR.ContainsKey(job)) {
						style.normal.textColor = _JOB_COLOUR[job];
					}

					EditorGUILayout.LabelField(job.Name, style);
					EditorGUILayout.LabelField("Current", job.Content, style);
					EditorGUILayout.LabelField("Paused", job.IsPaused.ToString(), style);
				}

				using(var isEnabled = new JuneDisabledGroup(isDisabled: false == job.ShouldRefresh)) {
					if(GUILayout.Button(job.PausePlayButtonText)) {
						job.TogglePause();
					}
				}
			}
		}
	}

	#endregion
}

public class JobDisplay {

	public Job Job;

	public string Name;

	public string Content;

	public bool IsPaused;

	public string PausePlayButtonText {
		get {
			return IsPaused ? "|>" : "||";
		}
	}

	public bool ShouldRefresh;

	/// <summary>
	/// Initializes a new instance of the <see cref="JobDisplay"/> class.
	/// </summary>
	/// <param name="job">Job.</param>
	public JobDisplay(Job job, string name) {
		this.Job = job;
		this.Name = name;
		this.ShouldRefresh = true;
		Refresh();
	}

	/// <summary>
	/// Toggles the pause.
	/// </summary>
	public void TogglePause() {
		if(null != Job && true == Job.IsPaused) {
			Job.UnPause();
		}
		else if(null != Job && false == Job.IsPaused) {
			Job.Pause();
		}
	}

	/// <summary>
	/// Refresh this instance.
	/// </summary>
	public void Refresh() {
		if(null != Job && ShouldRefresh) {
			try {
				if(this.Name.Contains("<null>")) {
					this.Name.Replace("<null>", GetJobType(Job));
				}
				this.Content = GetJobState();
				this.IsPaused = Job.IsPaused;
			}
			catch(Exception ex) { 
				ShouldRefresh = false; 
			}
		}
	}

	/// <summary>
	/// Gets the type of the job.
	/// </summary>
	/// <returns>The job type.</returns>
	/// <param name="job">Job.</param>
	public static string GetJobType(Job job) {
		if(null != job && null != job._Coroutine) {
			return job._Coroutine.GetType().Name;
		}
		return "<null>";
	}
	
	/// <summary>
	/// Gets the state of the job.
	/// </summary>
	/// <returns>The job state.</returns>
	/// <param name="job">Job.</param>
	private string GetJobState() {
		string state = "<null>";
		try {
			if(null != Job && null != Job._Coroutine && null != Job._Coroutine.Current) {
				state = RenderObject(Job._Coroutine.Current);
			}
		}
		catch(Exception ex) {
			//Debug.LogException(ex);
			ShouldRefresh = false;
		}
		return state;
	}

	/// <summary>
	/// Renders the object.
	/// </summary>
	/// <returns>The object.</returns>
	/// <param name="obj">Object.</param>
	private string RenderObject(object obj) {
		if(obj is WWW) {
			string url = ((WWW)obj).url;
			return string.Format("WWW: {0}", url);
		}
		else if(null != obj) {
			return obj.ToString();
		}
		return "<null>";
	}

}
