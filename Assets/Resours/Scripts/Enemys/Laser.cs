using System.Collections;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject player;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float waitDuration = 1f;

    private MeshRenderer laserRenderer;
    private Collider2D laserCollider;
    private Color originalColor;

    private void Awake()
    {
        laserRenderer = laser.GetComponent<MeshRenderer>();
        laserCollider = laser.GetComponent<Collider2D>();

        originalColor = laserRenderer.material.color;

       
        SetAlpha(0f);
        laserCollider.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(LaserPulse());
    }

    private IEnumerator LaserPulse()
    {
        while (true)
        {
           
            yield return FadeLaser(1f);
            laserCollider.enabled = true;

            yield return new WaitForSeconds(waitDuration);

            
            yield return FadeLaser(0f);
            laserCollider.enabled = false;

            yield return new WaitForSeconds(waitDuration);
        }
    }

    private IEnumerator FadeLaser(float targetAlpha)
    {
        float startAlpha = laserRenderer.material.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = originalColor;
        c.a = alpha;
        laserRenderer.material.color = c;
    }

    private void OnTriggerStay2D(Collider2D other)
{
    if (other.gameObject == player && laserCollider.enabled)
    {
        Destroy(player);
    }
}
}