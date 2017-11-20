using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This class implements the Pool Design Pattern.
/// </summary>
public class ObjectPool : MonoBehaviour {
    
    #region Public members

    public static ObjectPool instance;

    [Header("Pool item settings")]
    public GameObject pooledObject;
    public int numberOfPooledObjects = 2;

    [Header("Pool mode")]
    [Tooltip("If this is set to true the numberOfPooledObjects can be extended when needed.")]
    public bool isExtendable = true;
    [Tooltip("If this is set to true the first pooled object will be deactivated and reused for the last call to the pool.")]
    public bool isOverridable = false;

    #endregion

    #region Private members

    private IList<GameObject> pool;
    private int lastOverridableObjectIndex = 0;  //this index is used when isOverridable = true to keep track of the last object that was reused.

    #endregion

    void Awake () {
        instance = this;

        if ((isExtendable == true) && (isOverridable == true))
        {
            throw new System.Exception("Only one of the two modes can be set to true!");
        }
	}

    void Start()
    {
        pool = new List<GameObject>();
        for (int i = 0; i < numberOfPooledObjects; i++)
        {
            InstantiateNewPooledObject();
        }
    }

	/// <summary>
	/// This mathod pulls an object from the Pool.
	/// If there are no available objects one is either instantiated or reused.
	/// </summary>
	/// <returns>The pooled object.</returns>
    public GameObject GetPooledObject()
    {
        GameObject result = pool.Where(i => !i.activeInHierarchy).FirstOrDefault();

        if(result == null)
        {
            if (isExtendable)
            {
                result = InstantiateNewPooledObject();
            }
            else if (isOverridable)
            {
                result = pool.ElementAt(lastOverridableObjectIndex);

                if (lastOverridableObjectIndex == numberOfPooledObjects - 1)
                    lastOverridableObjectIndex = 0;
                else
                    lastOverridableObjectIndex++;
            }
        }
        
        return result;
    }

    public void DisableAllPooledObjects()
    {
        foreach (GameObject go in pool)
        {
            if (go.activeInHierarchy)
                go.SetActive(false);
        }
    }

    private GameObject InstantiateNewPooledObject()
    {
        GameObject result = Instantiate(pooledObject, transform);
        result.SetActive(false);
        pool.Add(result);

        return result;
    }

    private void Update()
    {
        foreach (var item in pool)
        {
            if(item == null)
                Debug.Log("There is a null Object in the Pool!");
        }
    }
}
