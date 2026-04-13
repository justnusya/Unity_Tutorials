using UnityEngine;

public class WeaponEffectsController : MonoBehaviour
{
    [Header("Налаштування клавіш")]
    public KeyCode toggleKey = KeyCode.Alpha1; 
    [Header("Ефекти")]
    public ParticleSystem weaponEffect;  
    public GameObject weaponLightsContainer;    
    [Header("Додаткові ефекти")]
    public ParticleSystem igniteVFX;    
    public ParticleSystem extinguishVFX; 

    private bool isActive = true;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleWeaponEffect();
        }
    }

    void ToggleWeaponEffect()
    {
        isActive = !isActive;

        if (weaponEffect != null)
        {
            if (isActive) weaponEffect.Play();
            else weaponEffect.Stop();
        }

        if (weaponLightsContainer != null)
        {
            weaponLightsContainer.SetActive(isActive);
        }

        if (isActive && igniteVFX != null) igniteVFX.Play();
        if (!isActive && extinguishVFX != null) extinguishVFX.Play();
    }
}