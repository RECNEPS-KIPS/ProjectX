
using UnityEngine;

public class ComboSFXtem : PoolItemBase
{
    [SerializeField]private ComboData comboData;
    private AudioSource audioSource;
    [SerializeField] private SoundStyle soundStyle;
    public void SetSoundStyle(SoundStyle soundStyle)
    {
        this.soundStyle =soundStyle;
    }
    public void GetComboData(ComboData comboData)
    { 
      this.comboData = comboData;
    }
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();  
    }
    protected override void Spawn()
    {
        base.Spawn();
       
        if (soundStyle == SoundStyle.ComboVoice)
        {
            audioSource.clip = comboData.characterVoice[Random.Range(0, comboData.characterVoice.Length)];
        }
        else if (soundStyle == SoundStyle.WeaponSound)
        {
            audioSource.clip = comboData.weaponSound[Random.Range(0, comboData.weaponSound.Length)];
        }
        if (audioSource.clip == null) { return; }
        audioSource.Play();
      
       
    }
    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            StopAudioPlay();
        }
    }
    private void StopAudioPlay()
    {
      this.gameObject.SetActive(false);
    }
}
