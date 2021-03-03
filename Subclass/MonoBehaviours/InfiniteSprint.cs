// <copyright file="InfiniteSprint.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.MonoBehaviours
{
    using Exiled.API.Features;
    using UnityEngine;

    public class InfiniteSprint : MonoBehaviour
    {
        private Player player;
        private new bool enabled = true;

        public void Destroy()
        {
            this.enabled = false;
            DestroyImmediate(this, true);
        }

        private void Awake()
        {
            this.player = Player.Get(this.gameObject);
        }

        private void Update()
        {
            if (this.enabled)
            {
                this.player.IsUsingStamina = false;
            }
        }
    }
}