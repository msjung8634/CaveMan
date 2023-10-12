using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy;
using TMPro;

public class Quest : MonoBehaviour
{
	private void Update()
	{
		CheckEnemyCount();
	}

	[SerializeField]
	private TextMeshProUGUI questDetailText;

	private void CheckEnemyCount()
	{
		EnemyStateMachine[] enemys = FindObjectsOfType<EnemyStateMachine>();
		if (enemys.Length == 0)
		{
			questDetailText.color = Color.green;
		}
	}
}
