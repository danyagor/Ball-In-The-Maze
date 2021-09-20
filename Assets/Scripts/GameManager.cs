using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityStandardAssets.Vehicles.Ball;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	[SerializeField] private int levelsInGame = 20;

	[SerializeField] private FollowingCamera followingCamera;
	[SerializeField] private MazeGenerator generator;

	[SerializeField] private GameObject ballPrefab;

	[SerializeField] private Transform killTrigger;

	[SerializeField] private GameObject hud;
	[SerializeField] private GameObject menuPanel;
	[SerializeField] private GameObject levelsPanel;
	[SerializeField] private GameObject finishPanel;

	[SerializeField] private GameObject levelsLayout;
	[SerializeField] private GameObject buttonPrefab;

	[SerializeField] private TextMeshProUGUI timeText;
	[SerializeField] private TextMeshProUGUI levelText;

	[SerializeField] private TextMeshProUGUI newRecordText;
	[SerializeField] private TextMeshProUGUI resultText;
	[SerializeField] private TextMeshProUGUI recordText;
	[SerializeField] private Button nextLevelButton;

	private float _gameTime;
	private GameObject _ballObject;
	private bool _gameStarted;
	private int _currentLevel;

	private float[] levelsRecords;

	private List<GameObject> panels;

	private void Start()
	{
		panels = new List<GameObject>();

		panels.Add(hud);
		panels.Add(menuPanel);
		panels.Add(levelsPanel);
		panels.Add(finishPanel);

		EnablePanelByName("Menu");

		// Get records from PlayerPrefs
		levelsRecords = new float[levelsInGame];
		for (int i = 0; i < levelsInGame; i++)
			levelsRecords[i] = PlayerPrefs.GetFloat("level_" + i, 0f);

		// Generate level buttons
		for (int i = 0; i < levelsInGame; i++)
		{
			GameObject obj = Instantiate(buttonPrefab, levelsLayout.transform);
			obj.GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
			int t = i;
			obj.GetComponentInChildren<Button>().onClick.AddListener(() => StartLevel(t));
		}

		generator.StartGeneration(20, 20, -1);
		followingCamera.MapCameraPosition = new Vector3(20 / 2, 5, 20 / 2);
		followingCamera.CameraMapActive = true;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			levelsPanel.SetActive(!levelsPanel.activeInHierarchy);

		if (_gameStarted && Input.GetKeyDown(KeyCode.M))
		{
			followingCamera.CameraMapActive = !followingCamera.CameraMapActive;
			_ballObject.GetComponent<BallUserControl>().enabled = !followingCamera.CameraMapActive;
		}

		if (_gameStarted)
		{
			_gameTime += Time.deltaTime;
			timeText.text = GetGameTimeString(TimeSpan.FromSeconds(_gameTime));
		}
	}

	public void EnablePanelByName(string name)
	{
		foreach (var panel in panels)
		{
			if (panel.name == name)
				panel.SetActive(true);
			else
				panel.SetActive(false);
		}
	}

	public void StartLevel(int level)
	{
		ResetGame();

		_currentLevel = level;
		int mazeSize = level + 5;

		_gameStarted = true;
		System.Random rand = new System.Random();
		int finishCellPosX = rand.Next(0, mazeSize);
		generator.StartGeneration(mazeSize, mazeSize, finishCellPosX);

		EnablePanelByName("GameHUD");

		GameObject obj = Instantiate(ballPrefab, new Vector3(finishCellPosX, 0.1f, 0), Quaternion.identity);
		_ballObject = obj;

		followingCamera.Target = obj.transform;
		followingCamera.MapCameraPosition = new Vector3(mazeSize / 2, mazeSize + 5, mazeSize / 2);
		followingCamera.CameraMapActive = false;
		_ballObject.GetComponent<BallUserControl>().enabled = true;

		float triggerPosition = (mazeSize / 2f) - 0.5f;
		killTrigger.position = new Vector3(triggerPosition, -10, triggerPosition);
		killTrigger.localScale = new Vector3(mazeSize + 10, 0.2f, mazeSize + 10);

		levelText.text = "Уровень " + (level + 1);
	}

	private void ResetGame()
	{
		_gameTime = 0f;
		generator.ClearMaze();

		if (_ballObject)
			Destroy(_ballObject);
	}

	public void GoToMenu(bool resetGame)
	{
		EnablePanelByName("Menu");

		if (resetGame)
		{
			ResetGame();
			generator.StartGeneration(20, 20, -1);
		}

		followingCamera.MapCameraPosition = new Vector3(20 / 2, 1, 20 / 2);
		followingCamera.CameraMapActive = true;
	}

	public void Finished()
	{
		EnablePanelByName("FinishPanel");

		_gameStarted = false;

		_ballObject.GetComponent<BallUserControl>().enabled = false;

		// Новый рекорд
		if (_gameTime < levelsRecords[_currentLevel] || levelsRecords[_currentLevel] == 0)
		{
			newRecordText.gameObject.SetActive(true);
			levelsRecords[_currentLevel] = _gameTime;
			PlayerPrefs.SetFloat("level_" + _currentLevel, _gameTime);
		}
		else // Нет рекорда
			newRecordText.gameObject.SetActive(false);

		recordText.text = "Рекорд: " + GetGameTimeString(TimeSpan.FromSeconds(levelsRecords[_currentLevel]));
		resultText.text = "Результат: " + GetGameTimeString(TimeSpan.FromSeconds(_gameTime));

		if (_currentLevel < levelsInGame - 1)
		{
			int nextLevelIndex = _currentLevel + 1;
			nextLevelButton.onClick.AddListener(() => { StartLevel(nextLevelIndex); });
		}
		else
			nextLevelButton.gameObject.SetActive(false);
	}

	private string GetGameTimeString(TimeSpan ts)
	{
		if (ts.Minutes > 0)
			return ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
		else
			return ts.Seconds.ToString("00");
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}
