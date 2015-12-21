using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Job manager. Proxy Object.
/// </summary>
public class JobManager : MonoBehaviour {

    private static JobManager _Instance = null;
    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static JobManager Instance {
        get {
            if (null == _Instance) {
                _Instance = FindObjectOfType(typeof(JobManager)) as JobManager;

                if (null == _Instance) {
                    var obj = new GameObject("JobManager");
                    _Instance = obj.AddComponent<JobManager>();
                }

                if (null != _Instance) {
                    DontDestroyOnLoad(_Instance.gameObject);
                }

                Dispatcher.Initialize();
            }
            return _Instance;
        }
    }

    /// <summary>
    /// Raised on the application quit event.
    /// </summary>
    void OnApplicationQuit() {
        //Release reference on quit
        _Instance = null;
    }

    public static void KillAllJobs() {
        JobManager.Instance.StopAllCoroutines();
    }
}

/// <summary>
/// Job.
/// </summary>
public class Job {

	#if UNITY_EDITOR || DEBUG
	public static List<Job> JOBS = new List<Job>();

	public static Action<Job> _OnJobStarted;
	public static Action<Job> _OnJobCompleted;
	#endif


    /// <summary>
    /// Occurs when on job complete.
    /// </summary>
    public event Action<bool> OnJobComplete;

    /// <summary>
    /// Gets a value indicating whether this instance is running.
    /// </summary>
    /// <value><c>true</c> if this instance is running; otherwise, <c>false</c>.</value>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is paused.
    /// </summary>
    /// <value><c>true</c> if this instance is paused; otherwise, <c>false</c>.</value>
    public bool IsPaused { get; private set; }

	#if UNITY_EDITOR || DEBUG
	public
	#else
	private 
	#endif 
	IEnumerator _Coroutine;

    private bool _IsJobKilled;
    private Stack<Job> _ChildJobStack;

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Job"/> class.
    /// </summary>
    /// <param name="coroutine">Coroutine.</param>
    public Job(IEnumerator coroutine)
        : this(coroutine, true) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Job"/> class.
    /// </summary>
    /// <param name="coroutine">Coroutine.</param>
    /// <param name="shouldStart">If set to <c>true</c> should start.</param>
    public Job(IEnumerator coroutine, bool shouldStart) {
        _Coroutine = coroutine;
		#if UNITY_EDITOR || DEBUG
		JOBS.Add(this);
		if(null != _OnJobStarted) {
			_OnJobStarted(this);
		}
		#endif
		if (shouldStart)
            Start();
    }

    /// <summary>
    /// Create the specified coroutine.
    /// </summary>
    /// <param name="coroutine">Coroutine.</param>
    public static Job Create(IEnumerator coroutine) {
        return new Job(coroutine);
    }

    /// <summary>
    /// Creates as coroutine.
    /// </summary>
    /// <returns>The as coroutine.</returns>
    /// <param name="coroutine">Coroutine.</param>
    public static IEnumerator CreateAsCoroutine(IEnumerator coroutine) {
        return new Job(coroutine, false).StartAsCoroutine();
    }

    #endregion

    #region Methods
    /// <summary>
    /// Creates the object and adds child job.
    /// </summary>
    /// <returns>The and add child job.</returns>
    /// <param name="coroutine">Coroutine.</param>
    public Job CreateAndAddChildJob(IEnumerator coroutine) {
        var j = new Job(coroutine, false);
        AddChildJob(j);
        return j;
    }

    /// <summary>
    /// Adds the child job.
    /// </summary>
    /// <param name="childJob">Child job.</param>
    public void AddChildJob(Job childJob) {
        if (null == _ChildJobStack) {
            _ChildJobStack = new Stack<Job>();
        }
        _ChildJobStack.Push(childJob);
    }

    /// <summary>
    /// Removes the child job.
    /// </summary>
    /// <param name="childJob">Child job.</param>
    public void RemoveChildJob(Job childJob) {
        if (_ChildJobStack.Contains(childJob)) {
            var childStack = new Stack<Job>(_ChildJobStack.Count - 1);
            var allCurrentChildren = _ChildJobStack.ToArray();
            Array.Reverse(allCurrentChildren);
            for (var i = 0; i < allCurrentChildren.Length; i++) {
                var j = allCurrentChildren[i];
                if (j != childJob) {
                    childStack.Push(j);
                }
            }
            _ChildJobStack = childStack;
        }
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    public void Start() {
        this.IsRunning = true;
#if UNITY_EDITOR
        if (false == Application.isPlaying) {
            try {
                while (_Coroutine.MoveNext()) {
                    var item = _Coroutine.Current;
                    if (item is WWW) {
                        var www = item as WWW;
                        // Wait till operation has completed
                        while (!www.isDone) ;
                    }
                }
            }
            catch (Exception ex) {
                Debug.Log("[JobManager] Start Exception " + ex.ToString());
            }
        }
        else {
            JobManager.Instance.StartCoroutine(DoWork());
        }
#else
		JobManager.Instance.StartCoroutine( DoWork() );
#endif
    }

    /// <summary>
    /// Starts as coroutine.
    /// </summary>
    /// <returns>The as coroutine.</returns>
    public IEnumerator StartAsCoroutine() {
        this.IsRunning = true;
        yield return JobManager.Instance.StartCoroutine(DoWork());
    }

    /// <summary>
    /// Pause this instance.
    /// </summary>
    public void Pause() {
        this.IsPaused = true;
    }

    /// <summary>
    /// Uns the pause.
    /// </summary>
    public void UnPause() {
        this.IsPaused = false;
    }

    /// <summary>
    /// Kill this instance.
    /// </summary>
    public void Kill() {
        this._IsJobKilled = true;
        this.IsRunning = false;
        this.IsPaused = false;
    }

    /// <summary>
    /// Kill the specified delayInSeconds.
    /// </summary>
    /// <param name="delayInSeconds">Delay in seconds.</param>
    public void Kill(float delayInSeconds) {
        var delay = (int)(delayInSeconds * 1000);
        new System.Threading.Timer(obj => {
            lock (this) {
                Kill();
            }
        }, null, delay, System.Threading.Timeout.Infinite);
    }

    #endregion

    /// <summary>
    /// Dos the work.
    /// </summary>
    /// <returns>The work.</returns>
    private IEnumerator DoWork() {
        //null our the first run through in case we start paused
        yield return null;

        while (IsRunning) {
            if (IsPaused) {
                yield return null;
            }
            else {
                //run the next iteration and stop if we are done
                if (_Coroutine.MoveNext()) {
                    yield return _Coroutine.Current;
                }
                else {
                    if (null != _ChildJobStack) {
                        yield return JobManager.Instance.StartCoroutine(RunChildJobs());
                    }
                    IsRunning = false;
                }
            }
        }

        //Fire OnComplete event
        if (null != OnJobComplete) {
            OnJobComplete(_IsJobKilled);
        }

		#if UNITY_EDITOR || DEBUG
		JOBS.Remove(this);
		if(null != _OnJobCompleted) {
			_OnJobCompleted(this);
		}
		#endif
    }

    /// <summary>
    /// Runs the child jobs.
    /// </summary>
    /// <returns>The child jobs.</returns>
    private IEnumerator RunChildJobs() {
        if (null != _ChildJobStack && _ChildJobStack.Count > 0) {
            do {
                Job childJob = _ChildJobStack.Pop();
                yield return JobManager.Instance.StartCoroutine(childJob.StartAsCoroutine());
            } while (_ChildJobStack.Count > 0);
        }
    }

    public override string ToString() {
        return string.Format("[Job: IsRunning={0}, IsPaused={1}]", IsRunning, IsPaused);
    }
}

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Dispatcher : MonoBehaviour {

    private static Dispatcher _instance;
    private static bool _initialized;
    private Queue<Action> _q = new Queue<Action>();

    public static Dispatcher Instance {
        get {
            if (!Dispatcher._initialized) {
                Debug.LogError("You need to call Initialize on the Main thread before using the Dispatcher");
                throw new InvalidOperationException("You need to call Initialize on the Main thread before using the Dispatcher");
            }
            return Dispatcher._instance;
        }
    }

    public static void Initialize() {
        if (Dispatcher._initialized) {
            return;
        }
        Dispatcher._initialized = true;
        GameObject gameObject = GameObject.Find(typeof(Dispatcher).Name);
        if (gameObject == null) {
            gameObject = new GameObject(typeof(Dispatcher).Name);
        }
        Dispatcher._instance = gameObject.GetComponent<Dispatcher>();
        if (Dispatcher._instance == null) {
            Dispatcher._instance = gameObject.AddComponent<Dispatcher>();
        }
        DontDestroyOnLoad(Dispatcher.Instance.gameObject);

#if UNITY_EDITOR
        if (false == Application.isPlaying) {
            UnityEditor.EditorApplication.update += Dispatcher.Instance.Update;
        }
#endif
    }

    private void Update() {
        // Message pump, executes only 1 action per cycle.
        if (this._q.Count > 0) {
            Action action = this._q.Dequeue();
            action();
        }
    }

    /// <summary>
    /// To the main thread.
    /// </summary>
    /// <param name="a">The alpha component.</param>
    public void ToMainThread(Action a) {
        this._q.Enqueue(a);
    }

    /// <summary>
    /// Launches the coroutine.
    /// </summary>
    /// <param name="firstIterationResult">First iteration result.</param>
    public void LaunchCoroutine(IEnumerator firstIterationResult) {
        Dispatcher.Instance.ToMainThread(delegate {
            Dispatcher.Instance.StartCoroutine(firstIterationResult);
        });
    }

    /// <summary>
    /// Executes the delayed.
    /// </summary>
    /// <param name="seconds">Seconds.</param>
    /// <param name="action">Action.</param>
    public void ToMainThreadAfterDelay(int seconds, Action action) {
        LaunchCoroutine(ExecuteDelayedEnumerator(seconds, action));
    }

    /// <summary>
    /// Executes on the background thread.
    /// </summary>
    /// <param name="action">Action.</param>
    public void ToBackgroundThread(Action action) {
        ThreadPool.QueueUserWorkItem(RunAsync, action);
    }

    /// <summary>
    /// Runs the async.
    /// </summary>
    /// <param name="action">Action.</param>
    private static void RunAsync(object action) {
        ((Action)action)();
    }

    /// <summary>
    /// Executes the delayed enumerator.
    /// </summary>
    /// <returns>The delayed enumerator.</returns>
    /// <param name="seconds">Seconds.</param>
    /// <param name="action">Action.</param>
    private IEnumerator ExecuteDelayedEnumerator(int seconds, Action action) {
        yield return new WaitForSeconds((float)seconds);
        action();
    }
}