using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UIElements;
using UnityEditor;

public class FireController : MonoBehaviour
{
    public float headDamage = 100f;
    public float torsoArmDamage = 25f;
    public float range = 100f;
    public Camera fpsCam;
    public float fireRate = 1000f;
    public int maxAmmo = 10;
    public float reloadTime = 1.8f;
    
    public Animator animator;
    public ParticleSystem muzzle;
    public TextMeshProUGUI ammoDisp;
    public GameObject impact;
    public GameObject enemyHit;
    
    private AudioManager audioManager;

    private int currentAmmo = -1;
    private float nextTime = 0f;
    
    private bool reloading = false;
    private bool isFiring = false;
    private bool isADS = false;
    
    void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }


    void Start()
    {
        if (currentAmmo == -1)
        {
            currentAmmo = maxAmmo;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ammoDisp.text = currentAmmo.ToString();
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo && !isFiring && !reloading)
        {
            StartCoroutine(Reload());
            return;
        }
        else if (currentAmmo == 0 && !reloading)
        {
            audioManager.Stop("FireSound");
            animator.SetBool("isMouse", false);
            animator.SetBool("Crouching", true);
            isFiring = false;
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && currentAmmo > 0 && !reloading)
        {
            
            isFiring = true;
            animator.SetBool("isMouse", isFiring);
            animator.SetBool("Crouching", false);
            if(Time.time >= nextTime)
            {
                if (!audioManager.isPlaying("FireSound"))
                {
                    audioManager.Play("FireSound");
                }
                Shoot();
                nextTime = Time.time + 1f / fireRate;
            }   

        }
        else if (currentAmmo == 0 || !Input.GetButton("Fire1"))
        {
            audioManager.Stop("FireSound");
            animator.SetBool("isMouse", false);
            animator.SetBool("Crouching", true);
            isFiring = false;
        }
        

        if (Input.GetButton("Fire2"))
        {
            isADS = true;
            animator.SetBool("isADS", isADS);
            animator.SetBool("Crouching", false);

        }
        else
        {
            isADS = false;
            animator.SetBool("isADS", isADS);
            animator.SetBool("Crouching", true);
        }

    }
    void Shoot()
    {
        muzzle.Play();
        currentAmmo -= 1;
        RaycastHit hit;
       if(Physics.Raycast(fpsCam.transform.position,fpsCam.transform.forward,out hit, range))
        {

            //Debug.Log(hit.transform.name);
            HitEnemy hitEnemy = hit.transform.GetComponent<HitEnemy>();
            if (hitEnemy != null)
            {
                if (hit.transform.name.Equals("HeadHitMarker"))
                {
                    hitEnemy.Damage(headDamage);
                }
                else
                {
                    
                    hitEnemy.Damage(torsoArmDamage);
                }
                GameObject go1 = Instantiate(enemyHit, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(go1, 2f);
            }
            else
            {
                GameObject go2 = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(go2, 2f);
            }
        }
    }

    IEnumerator Reload()
    {
        FindObjectOfType<AudioManager>().Play("ReloadSound");
        reloading = true;
        animator.SetBool("Crouching", false);
        animator.SetBool("isReload", true);
        yield return new WaitForSecondsRealtime(0.25f);
        animator.SetBool("isReload", false);
        yield return new WaitForSecondsRealtime(reloadTime);
        currentAmmo = maxAmmo;
        animator.SetBool("Crouching", true);
        reloading = false;
    }

    
}
