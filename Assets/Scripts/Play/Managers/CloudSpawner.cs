using UnityEngine;
using System.Collections;

public class CloudSpawner : MonoBehaviour
{
    public CloudOptions[] cloudOptionss;		    // Array of cloud options
    public GameObject BaseCloud;
    public float cloudDelayTime = 2f;		// Delay on delivery.
    public float RangeLeft;					// Smallest value of x in world coordinates the delivery can happen at.
    public float RangeRight;				// Largest value of x in world coordinates the delivery can happen at.

    void Start()
    {
        // Start the first delivery.
        StartCoroutine(CloudSpawn());
    }


    public IEnumerator CloudSpawn()
    {
        while (true) //Go on forever!! (This is temporary until I find a better thing to write here.
        {
            // Wait for the delivery delay.
            yield return new WaitForSeconds(cloudDelayTime);

            // Create a random x coordinate for the delivery in the drop range.
            float dropPosX = Random.Range(RangeLeft, RangeRight);

            // Create a position with the random x coordinate.
            Vector3 dropPos = new Vector3(dropPosX, -37f, 1f);
            int cloudIndex = Random.Range(0, cloudOptionss.Length);
            // ... instantiate the base cloud at the drop position.
            GameObject cloud = Instantiate(BaseCloud, dropPos, Quaternion.identity) as GameObject;
            cloudOptionss[cloudIndex].SetupCloud(cloud);
            Destroy(cloud, 2f);
        }


    }
}
