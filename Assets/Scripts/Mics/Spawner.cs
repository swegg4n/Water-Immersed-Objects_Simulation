using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private int n = 100;
    [SerializeField] private float delay = 0.1f;
    [SerializeField] private float initialDelay = 0.0f;

    [SerializeField] private bool random = true;


    private void Start()
    {
        StartCoroutine(RepeatedSpawn());
    }


    private IEnumerator RepeatedSpawn()
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < n; i++)
        {

            Vector3 pos = (random) ?
                transform.position + new Vector3(
                Random.Range(-10.0f, 10.0f),
                Random.Range(-10.0f, 10.0f),
                Random.Range(-10.0f, 10.0f)
                )
                : transform.position;

            Quaternion rot = (random) ?
                Quaternion.Euler(
                    Random.Range(0.0f, 360.0f),
                    Random.Range(0.0f, 360.0f),
                    Random.Range(0.0f, 360.0f)
                    )
                : Quaternion.identity;

            Instantiate(spawnPrefab, pos, rot, transform);

            yield return new WaitForSeconds(delay);
        }
    }

}
