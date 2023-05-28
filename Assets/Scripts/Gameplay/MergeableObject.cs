using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MergeableObject : DragAndDropable
{
    [Header("Mergeable Object Settings")]
    [SerializeField] private int _level;
    [SerializeField] private GameObject _nextLevelObject;

    private MergeablePanel _parentPanel;
    private MergeablePanel _hoveredPanel;

    protected bool _haveRaycast = false;
    protected RaycastHit _currentHit;

    public int GetLevel() => _level;
    
    protected override void Drag()
    {
        base.Drag();
        
        LayerMask mask = LayerMask.GetMask("ZombiePanel");
        Ray ray = new Ray(transform.position, transform.up * -1);

        _haveRaycast = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, mask);
        if (_haveRaycast)
        {
            _currentHit = hit;
            if (hit.collider.TryGetComponent(out MergeablePanel panel))
            {
                if (panel.IsPanelFree()) return;
                if (panel.GetObject().GetLevel() != _level) return;
                if (panel == _parentPanel) return;

                _hoveredPanel = panel;
                print("found another zombie");
            }
            
            else if (_hoveredPanel != null) _hoveredPanel = null;
        }
    }

    protected override void Drop()
    {
        Vector3 newObjectPos;
        
        if (!_hoveredPanel)
        {
            newObjectPos = new Vector3(_parentPanel.transform.position.x,
                _parentPanel.transform.position.y + 1, _parentPanel.transform.position.z);

            transform.position = newObjectPos;
            return;
        }

        newObjectPos = new Vector3(_hoveredPanel.transform.position.x,
            _hoveredPanel.transform.position.y + 1, _hoveredPanel.transform.position.z);
        
        GameObject newObject = Instantiate(_nextLevelObject, newObjectPos,
            _hoveredPanel.transform.rotation, _hoveredPanel.transform);

        MergeableObject mergeableObjectComponent = newObject.GetComponent<MergeableObject>();
        mergeableObjectComponent.SetParentPanel(_hoveredPanel);
        
        _parentPanel.ClearObject();

        Destroy(_hoveredPanel.GetObject().gameObject);
        _hoveredPanel.ClearObject();
        _hoveredPanel.SetObject(mergeableObjectComponent);

        Destroy(gameObject);
    }

    public void SetParentPanel(MergeablePanel mergeablePanel) => _parentPanel = mergeablePanel;
    public MergeablePanel GetParentPanel() => _parentPanel;
}
