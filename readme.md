# System

A Damage Tracking state-machine system for mechs in a heroic combat game

## Intent

My goal is to a describe a system that takes into account meaningful consequences for mech damage without the use of scalar values, or the use of random selection for meaningful decision making.  It is advised by my experience researching systems for Real-World Battle Damage Assessment or "BDA".  Further, this system is intended to be fun, meaning that events that occur within the system should enable the player to make decisions that they feel have an impact on how the game proceeds.

Here are the additional constraints that I'm placing on myself
* Bullet-Sponges aren't fun
  * When I fire a weapon, something meaningful should happen.  I shouldn't need to fire endlessly on a target that is vulnerable to my attack in order to achieve a meaningful effect
* Backpedalling isn't fun
  * This is a corollary to bullet-sponges, but in addition, I should be able to pick a plan of attack and execute, knowing full well that if it fails, I fail.  A strategy that requires running backwards while my opponent ineffectually chases me undermines my ability to feel meaningfully connected to the game world.
* Suspense is fun
  * While too much suspense can be exhausting or boring, the right amount of suspense means there's always something that I want to happen, and I don't want to happen.  This system should promote the activity of being able to constantly make meaningful tradeoffs, without inducing "decision paralysis"
* Heroes make sacrifices
  * In western culture at least, we have a clear notion of a hero's journey, and the traits that define a hero.  It seems like the games that players really dig into are the ones where characters have to give up something for the greater good.  As part of that, I think that strategic movements should come with a meaningful cost.  My experience working with human beings in an engineering setting has taught me that people understand qualitative variables a lot better than quantitative, so I want to give the player the opportunity to deal with fundamental and sometimes irreversible consequences to their actions.
* Complex, but easy
  * Players like complexity because it gives them things to consider and discuss, but dislike when that complexity requires them to do things they don't care about.  Complex systems should be able to automatically make good decisions for themselves about how to solve problems, but players should have the ability to override those decisions if they want better behavior
* It has to be fast at scale
  * Combat happens fast, which means that events that need to be processed during combat should be non-blocking, parallelizable, and resolvable in constant time (at least on the main simulation thread)
* Inferrable, but not predictable
  * Generally, the problem with using random numbers for decision making is that the player loses the ability to draw inferences about what will happen when they take an action.  The player should be able to model in their mind roughly what will happen when they take an action but, that model is more fun when it cannot be perfectly precise, because it amplifies suspense.

## System elements

### Structural Components

A mech is ultimately composed of *Structural Components*. These can come in many types, for example:
* *Core*
* *Upper Leg*
* *Forearm*
* *Weapon*  

*Structural Components* have *Hardpoints* that allow for the attachment of other *Structural Components*, but those hardpoints may be limited in the types of components that they can accept as mates.  For example:

* An *Simple Upper Leg* can accept accept 2 mated *Structural Components* through its *Structure Hardpoints* a *Hip Joint*, and a *Knee Joint*.  
* A *Rigid Forearm* can accept 2 mated *Structural Components* through its *Structure Hardpoints*, an *Elbow Joint*, and a *Fixed Weapon* or *Wrist Joint*
* A *Shoulder Joint* can accept 2 mated components through its *Structure Hardpoints*, an *Upper Arm*, and a *Shoulder*.  It can also accept an *Actuator* through its *System Hardpoint*

### Structure and Channels

The first concept in this system is that every *Structural Component* of a mech has two functions, *Structure* which provides the general form of the mech, and *Channels* which allow for the transportation of resources.

Some examples of *Structure* are:
* The mech's legs are structurally sound, so it is standing
* The structure of the mech's foot has been destroyed, so it can continue to limp
* The mech's arm has been destroyed, so the minigun that was attached to it is no longer usable

Some examples of *Channels* are:
* The mech's knee actuator requires both power and data in order to function, or else it does not working
* The acoustic sensors on a mech's foot only works when it has an active data connection to the core, and to the sensor on the other foot
* The minigun on the mech's arm has run out of ammo, but there is more ammo in storage on the mech's core, a transport channel must be available to transfer the ammo from the core to the weapon so that it can continue to fire

### Damage and Armor Classes

Weapons produce damage that is categorized through *Damage Classes* a damage class is natural number that represents the magnitude of damage that the weapon can do to a physical system.  In order for a weapon a weapon to in any way impact a system, its *Damage Class* must exceed the *Armor Class* of the system that it is targeting.  This means that **Small Weapons Fire** which might have a *Damage Class* of 1, can not affect an *Heavy Composite Armor Upper Leg* that has an *Armor Class* of 2, but it can affect an *Exposed Knee Joint* which has an armor class of 0.

When the *Damage Class* of a weapon exceeds the *Armor Class* of a *Structural Component*, the effect is partial destruction of that component.  The destruction proceeds in 3 levels, which are determined by calculating *Damage Class* - *Armor Class*
1. Destruction of *Channels*
2. Destruction of *Mounted Systems* and *Channels*
3. Destruction of *Structure*, *Mounted Systems*, and *Channels*

When the *Damage Class* exceeds the *Armor Class* a few things happen:

First, the remaining damage after the *Armor Class* is subtracted is used to determine the magnitude of the damage that is done.  The following rules are applied in order

> For each remaining damage, a channel is randomly selected from the *Structural Component* and destroyed.  It the channel was already destroyed, there is no effect

> For every 2 remaining damage, a *Mounted System* is randomly selected, and 2 damage is applied to it

> For every 3 remaining damage, a *Structure* is randomly selected and destroyed.  If this was the last undestroyed  *Structures*, the *Structural Component* is destroyed

When a *Structural Component* is destroyed, all of the connected components are lost

### Example

Justin has engaged Chris in combat. He feels pretty good about his odds in a 1:1 fight, but he's worried that Chris's lighter, faster mech will escape the engagement and regroup with his friends.  He knows that Chris has to be getting low on jump fuel, so he decides to take out his ability to escape on foot.

Justin is pretty sure he understands Chris's build, and he's pretty confident that his left knee is actually an *Exposed Knee Joint* with an *Armor Class* of 0.  Justin has three weapons that he can make use of:
1. A *Light Machine Gun* with a *Damage Class* of 1
2. A *40mm Recoilless Rifle* with a *Damage Class* of 2
3. A *Light Gauss Cannon* with a *Damage Class* of 4

Since he's got plenty of *Light Machine Gun* ammo, he decides to target the knee joint and fire a volley of rounds.  His HUD informs him that the volley hit, but that no damage was detected.  That means that the knee joint wasn't in fact an *Exposed Knee Joint* but was instead some knee joint with armor.

Chris hasn't gotten away yet though, and before he does Justin decides to take another shot at this joint with his recoilless rifle.  The system registers that hit induced damage.

Because there is an extra damage after subtracting
