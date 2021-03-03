// <copyright file="AbilityType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass
{
    public enum AbilityType
    {
        /// <summary>
        /// The ability to pry gates open.
        /// </summary>
        PryGates,

        /// <summary>
        /// Grants invulnerability.
        /// </summary>
        GodMode,

        /// <summary>
        /// Makes the player invisible to other users until they interact with an object.
        /// </summary>
        InvisibleUntilInteract,

        /// <summary>
        /// Makes the player not require a keycard to get past locked doors.
        /// </summary>
        BypassKeycardReaders,

        /// <summary>
        /// 
        /// </summary>
        HealGrenadeFrag,

        /// <summary>
        /// 
        /// </summary>
        HealGrenadeFlash,

        /// <summary>
        /// The user does not trigger tesla gates.
        /// </summary>
        BypassTeslaGates,

        /// <summary>
        /// The users sprint meter does not drain.
        /// </summary>
        InfiniteSprint,

        /// <summary>
        /// The user cannot enrage Scp096.
        /// </summary>
        Disable096Trigger,

        /// <summary>
        /// The user cannot prevent Scp173 from moving.
        /// </summary>
        Disable173Stop,

        /// <summary>
        /// 
        /// </summary>
        Revive,

        /// <summary>
        /// 
        /// </summary>
        Echolocate,

        /// <summary>
        /// The user gains Scp939's vision effect.
        /// </summary>
        Scp939Vision,

        /// <summary>
        /// The users AHP does not decrease over time.
        /// </summary>
        NoArmorDecay,

        /// <summary>
        /// 
        /// </summary>
        NoClip,

        /// <summary>
        /// The user does not take damage from Scp207.
        /// </summary>
        NoSCP207Damage,

        /// <summary>
        /// The user does not take damage from Scps.
        /// </summary>
        NoSCPDamage,

        /// <summary>
        /// The user does not take damage from humans.
        /// </summary>
        NoHumanDamage,

        /// <summary>
        /// The user has infinite ammunition.
        /// </summary>
        InfiniteAmmo,

        /// <summary>
        /// 
        /// </summary>
        Nimble,

        /// <summary>
        /// 
        /// </summary>
        Necromancy,

        /// <summary>
        /// The user does not get affected by flash grenades.
        /// </summary>
        FlashImmune,

        /// <summary>
        /// The user does not get affected by frag grenades.
        /// </summary>
        GrenadeImmune,

        /// <summary>
        /// The user cannot enter Scp106's femur breaker.
        /// </summary>
        CantBeSacraficed,

        /// <summary>
        /// The user cannot press the button to trigger the femur breaker sequence.
        /// </summary>
        CantActivateFemurBreaker,

        /// <summary>
        /// 
        /// </summary>
        LifeSteal,

        /// <summary>
        /// 
        /// </summary>
        Zombie106,

        /// <summary>
        /// 
        /// </summary>
        FlashOnCommand,

        /// <summary>
        /// 
        /// </summary>
        GrenadeOnCommand,

        /// <summary>
        /// 
        /// </summary>
        ExplodeOnDeath,

        /// <summary>
        /// The user can become invisible upon use of a command.
        /// </summary>
        InvisibleOnCommand,

        /// <summary>
        /// 
        /// </summary>
        Disguise,

        /// <summary>
        /// The user cannot regain health.
        /// </summary>
        CantHeal,

        /// <summary>
        /// The user heals those around them.
        /// </summary>
        HealAura,

        /// <summary>
        /// The user damages those around them.
        /// </summary>
        DamageAura,

        /// <summary>
        /// The user regains health over time.
        /// </summary>
        Regeneration,

        /// <summary>
        /// 
        /// </summary>
        Infect,

        /// <summary>
        /// 
        /// </summary>
        BackupCommand,

        /// <summary>
        /// 
        /// </summary>
        Vent,

        /// <summary>
        /// 
        /// </summary>
        PowerSurge,

        /// <summary>
        /// 
        /// </summary>
        Summon,

        /// <summary>
        /// 
        /// </summary>
        CantBeInfected,

        /// <summary>
        /// 
        /// </summary>
        Punch,

        /// <summary>
        /// 
        /// </summary>
        Stun,

        /// <summary>
        /// 
        /// </summary>
        Bloodlust,

        /// <summary>
        /// 
        /// </summary>
        Disarm,

        /// <summary>
        /// 
        /// </summary>
        Fake,

        /// <summary>
        /// 
        /// </summary>
        Corrupt,

        /// <summary>
        /// 
        /// </summary>
        Multiply,

        /// <summary>
        /// The user cannot escape the facility through the surface tunnel.
        /// </summary>
        CantEscape,
    }
}