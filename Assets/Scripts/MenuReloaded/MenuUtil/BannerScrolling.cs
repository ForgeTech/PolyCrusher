using UnityEngine;
using UnityEngine.UI;
using System.Text;

[RequireComponent(typeof(Text))]
public class BannerScrolling : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField]
    private string word = "Test - ";
    private int currentWordIndex = 0;

    [SerializeField]
    private int iterations = 2;

    [SerializeField]
    private float moveEveryXSeconds = 0.150f;
    private float currentTime = 0f;

    private StringBuilder displayString;
    private Text textComponent;

    private void Start()
    {
        int size = word.Length * iterations;
        displayString = new StringBuilder(size, size);

        for (int i = 0; i < iterations; i++)
            displayString.Append(word);

        textComponent = GetComponent<Text>();
        textComponent.text = displayString.ToString();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (currentTime > moveEveryXSeconds)
        {
            MakeStringConcatenation();
            textComponent.text = displayString.ToString();
            currentTime = 0f;
        }

        currentTime += Time.deltaTime;
    }

    private void MakeStringConcatenation()
    {
        // Remove First
        displayString.Remove(0, 1);

        // Add last
        displayString.Insert(displayString.Length - 1, word[currentWordIndex % word.Length]);
        currentWordIndex++;
    }
}