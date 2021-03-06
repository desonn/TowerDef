using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//namespace GameDevHQ.FileBase.Dual_Gatling_Gun

    /// <summary>
    /// This script will allow you to view the presentation of the Turret and use it within your project.
    /// Please feel free to extend this script however you'd like. To access this script from another script
    /// (Script Communication using GetComponent) -- You must include the namespace (using statements) at the top. 
    /// "using GameDevHQ.FileBase.Dual_Gatling_Gun" without the quotes. 
    /// 
    /// For more, visit GameDevHQ.com
    /// 
    /// @authors
    /// Al Heck
    /// Jonathan Weinberger
    /// </summary>

    [RequireComponent(typeof(AudioSource))] //Require Audio Source component
    public class Dual_Gatling_Gun : MonoBehaviour
    {
        [SerializeField]
        private Transform[] _gunBarrel; //Reference to hold the gun barrel
        [SerializeField]
        private GameObject[] _muzzleFlash; //reference to the muzzle flash effect to play when firing
        [SerializeField]
        private ParticleSystem[] _bulletCasings; //reference to the bullet casing effect to play when firing
        [SerializeField]
        private AudioClip _fireSound; //Reference to the audio clip

        private AudioSource _audioSource; //reference to the audio source component
        private bool _startWeaponNoise = true;
        [SerializeField]
        Transform target;
        [SerializeField]
        Transform partToRotate;
        SpawnManager spawnList;
        float turnSpeed = 8;
        [SerializeField]
        bool inRange;
        EnemyClass enemyClass;
        public List<GameObject> robots = new List<GameObject>();
        public int gunID;

        // Use this for initialization
        void Start()
        {
        enemyClass = GetComponent<EnemyClass>();
        spawnList = SpawnManager.Instance;
        _muzzleFlash[0].SetActive(false); //setting the initial state of the muzzle flash effect to off
            _muzzleFlash[1].SetActive(false); //setting the initial state of the muzzle flash effect to off
            _audioSource = GetComponent<AudioSource>(); //ssign the Audio Source to the reference variable
            _audioSource.playOnAwake = false; //disabling play on awake
            _audioSource.loop = true; //making sure our sound effect loops
            _audioSource.clip = _fireSound; //assign the clip to play
     
    }

        // Update is called once per frame
        void Update()
        {
        if (target == null)
        {
            //for loop to iterate through all muzzle flash objects
            for (int i = 0; i < _muzzleFlash.Length; i++)
            {
                _muzzleFlash[i].SetActive(false); //enable muzzle effect particle effect
            }
            _audioSource.Stop(); //stop the sound effect from playing
            _startWeaponNoise = true; //set the start weapon noise value to true
            return;
        }
        if (inRange == true) //Check for left click (held) user input
            { 
                RotateBarrel(); //Call the rotation function responsible for rotating our gun barrel

                //for loop to iterate through all muzzle flash objects
                for(int i = 0; i < _muzzleFlash.Length; i++)
                {
                    _muzzleFlash[i].SetActive(true); //enable muzzle effect particle effect
                    _bulletCasings[i].Emit(1); //Emit the bullet casing particle effect   
                }

                if (_startWeaponNoise == true) //checking if we need to start the gun sound
                {
                    _audioSource.Play(); //play audio clip attached to audio source
                    _startWeaponNoise = false; //set the start weapon noise value to false to prevent calling it again
                }

            }
            else if (inRange == false) //Check for left click (release) user input
            {
                //for loop to iterate through all muzzle flash objects
                for (int i = 0; i < _muzzleFlash.Length; i++)
                {
                    _muzzleFlash[i].SetActive(false); //enable muzzle effect particle effect
                }
                _audioSource.Stop(); //stop the sound effect from playing
                _startWeaponNoise = true; //set the start weapon noise value to true
            }
        TurnBarrel();
        }

        // Method to rotate gun barrel 
        void RotateBarrel() 
        {
            _gunBarrel[0].transform.Rotate(Vector3.forward * Time.deltaTime * -500.0f); //rotate the gun barrel along the "forward" (z) axis at 500 meters per second
            _gunBarrel[1].transform.Rotate(Vector3.forward * Time.deltaTime * -500.0f); //rotate the gun barrel along the "forward" (z) axis at 500 meters per second
        }
    void TurnBarrel()
    {
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

    }
    public void UpdateTarget()
    {
        // update target sort closest in list 


        float shortestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        foreach (var r in robots)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, r.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = r;
            }
            if (closestEnemy != null)
            {
                target = closestEnemy.transform;
                inRange = true;

            }
            else
            {
                target = null;
                inRange = false;

            }

        }


    }
    private void ResetTartget()
    {
        target = null;
    }
    private IEnumerator OnTriggerStay(Collider other)
    {

        IDamage Dam = other.GetComponent<IDamage>();




        if (other.tag == "Robot")
        {

            if (Dam != null)
                UpdateTarget();
            Dam.Damage();

            yield return new WaitForSeconds(3);
        }
        if (Dam.Health < 1)
        {
            robots.Remove(other.gameObject);
            inRange = false;
            target = null;

        }

    }

    private void OnTriggerEnter(Collider other)
    {
        robots.Add(other.gameObject);

    }
    private void OnTriggerExit(Collider other)
    {
        robots.Remove(other.gameObject);

    }
    private void OnEnable()
    {
        EnemyClass.Dying += ResetTartget;

    }
    private void OnDisable()
    {
        EnemyClass.Dying -= ResetTartget;


    }
}

