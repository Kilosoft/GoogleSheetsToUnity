using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        var persons = ManagerStaticData.CreateRuntimeInstance<PersonModel, PersonsStaticModel>();
        Debug.LogError(persons.Persons.Count);
    }
}
