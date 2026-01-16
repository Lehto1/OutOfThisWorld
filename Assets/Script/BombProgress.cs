using TMPro;
using UnityEngine;

public class BombProgress : MonoBehaviour
{
    // Bomb relaterat variabler
    [SerializeField] TextMeshProUGUI BombText;
    public int BombsLeft;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BombsLeft = 3;
    }

    // Update is called once per frame
    void Update()
    {
        BombText.text = "Bombs left " + BombsLeft.ToString(); // Gör så att texten visar mängden bombar kvar
    }
}
