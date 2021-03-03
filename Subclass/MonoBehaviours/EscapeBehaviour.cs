// <copyright file="EscapeBehaviour.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.MonoBehaviours
{
    using Exiled.API.Features;
    using Mirror;
    using UnityEngine;

    public class EscapeBehaviour : NetworkBehaviour
    {
        private Player player;
        private Vector3 escapePosition;
        private new bool enabled = true;

        public RoleType EscapesAsCuffed { get; set; } = RoleType.None;

        public RoleType EscapesAsNotCuffed { get; set; } = RoleType.None;

        public void Destroy()
        {
            this.enabled = false;
            DestroyImmediate(this, true);
        }

        private void Awake()
        {
            this.player = Player.Get(this.gameObject);
            this.escapePosition = this.GetComponent<Escape>().worldPosition;
        }

        private void Update()
        {
            if (this.enabled)
            {
                if (Vector3.Distance(this.transform.position, this.escapePosition) < Escape.radius)
                {
                    if (!this.player.IsCuffed && this.EscapesAsNotCuffed != RoleType.None)
                    {
                        this.player.SetRole(this.EscapesAsNotCuffed, false, true);
                    }
                    else if (this.player.IsCuffed && this.EscapesAsCuffed != RoleType.None)
                    {
                        this.player.SetRole(this.EscapesAsCuffed, false, true);
                    }
                }
            }
        }
    }
}