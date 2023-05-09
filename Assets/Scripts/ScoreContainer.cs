using TMPro;
using UnityEngine;

public class ScoreContainer : MonoBehaviour
{
    public static ScoreContainer Instance { get; private set; }

    private int _score;
    public int score
    {
        get => _score;
        set
        {
            _score = value;
            scoreText.SetText($"Pontos: {_score}");
        }
    }

    [SerializeField] private TextMeshProUGUI scoreText;


    private void Awake() => Instance = this;

}
