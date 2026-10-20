using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] Animator _anim;
    [SerializeField] ParticleSystem[] _particles;

    [SerializeField] string _dropTableID = "";
    [SerializeField] float _dropRadius = 3f;
    [SerializeField] GameObject _defaultDropItem;

    int _hashOpen = Animator.StringToHash("Open");
    void Awake()
    {
        if ( _anim == null ) _anim = GetComponent<Animator>();
        if (_particles == null) _particles = GetComponentsInChildren<ParticleSystem>();

        if( _particles.Length > 0)
        {
            foreach( var particle in _particles )
            {
                particle.Stop();
            }
        }
    }

    void Start()
    {
        
    }

    public bool Interact()
    {
        if(_anim == null ) return false;
        if( _particles == null ) return false;

        _anim.SetTrigger(_hashOpen);
        foreach (var particle in _particles)
        {
            particle.Play();
        }

        Drop();
        Destroy(gameObject, 3f);
        return false;
    }

    public void Drop()
    {
        var dropList = DataManager.Instance.GetDropTable(_dropTableID);
        if (dropList == null) return;

        int count = dropList.Count;

        for (int i = 0; i < count; i++)
        {
            float rate = Random.Range(0f, 1f);
            if (rate <= dropList[i].DropRate)
            {
                Vector2 offset = Vector2.down * _dropRadius;
                Vector3 pos = new Vector3(transform.position.x + offset.x, transform.position.y, transform.position.z + offset.y);
                GameObject item = Instantiate(_defaultDropItem, pos, Quaternion.identity);

                int amount = Random.Range(dropList[i].MinAmount, dropList[i].MaxAmount + 1);
                if (item.TryGetComponent(out WorldItem worldItem))
                {
                    worldItem.Init(dropList[i].ItemData, amount);
                }
                else
                {
                    item.AddComponent<WorldItem>().Init(dropList[i].ItemData, amount);
                }
            }
        }
    }

}
