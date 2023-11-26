using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WhiteboardMarker : MonoBehaviour
{
    [SerializeField] private Transform _tip;
    [SerializeField] public int _penSize;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;

    // Color del tablero
    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogError("Renderer not found on _tip. Make sure the _tip object has a Renderer component.");
            return;
        }

        //_penSize = 5;

        Debug.Log(_renderer.material.color);

        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();

        _tipHeight = _tip.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (material == null)
        {
            Debug.LogError("Material is not assigned.");
            return;
        }

        material.color = material.color;
        Material mati = new Material(material);
        SwitchColorG(mati.color);
        Draw();
    }

    private void Draw()
    {
        if (Physics.Raycast(origin:_tip.position, direction:transform.up, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

                if (y < 0 || y > _whiteboard.textureSize.y || x < 0 || x > _whiteboard.textureSize.x) return;

                if (_touchedLastFrame)
                {
                    _whiteboard.texture.SetPixels(x, y, blockWidth:_penSize, blockHeight:_penSize, _colors);

                    for (float f = 0.01f; f < 1.00f; f += 0.01f)
                    {
                        var lerpX = (int)Mathf.Lerp(a:_lastTouchPos.x, b:x, t:f);
                        var lerpY = (int)Mathf.Lerp(a:_lastTouchPos.y, b:y, t:f);
                        _whiteboard.texture.SetPixels(lerpX, lerpY, blockWidth:_penSize, blockHeight:_penSize, _colors);
                    }

                    transform.rotation = _lastTouchRot;

                    _whiteboard.texture.Apply();
                }

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        _whiteboard = null;
        _touchedLastFrame = false;
    }

    private void SwitchColorG(Color color)
    {
        if (_renderer == null)
        {
            Debug.LogError("_renderer is not initialized.");
            return;
        }

        _renderer.material.color = color;
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        //Debug.Log("a");
    }

    public void grosor(int label)
    {
        if (label == 0)
        {
            _penSize += 1;
        }
        else if(label == 1)
        {
            _penSize -= 1;
        }
    }
}