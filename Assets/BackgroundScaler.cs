using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    private void Start()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();

        var camera = Camera.main;
        var height = camera.orthographicSize * 2;
        var width = height * camera.aspect;

        var spriteSize = spriteRenderer.bounds.size;
        var scale = new Vector3(
            width / spriteSize.x,
            height / spriteSize.y,
            1f
            );
        var maxScale = Mathf.Max(scale.x, scale.y);
        transform.localScale = Vector3.one * maxScale;
    }
}