using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void Leaderboard()
    {
        SceneManager.LoadScene("Leaderboard");
    }
    public void Main()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Shop()
    {
        SceneManager.LoadScene("Shop");
    }
    public void Exit()
    {
        Application.Quit();
    }
}