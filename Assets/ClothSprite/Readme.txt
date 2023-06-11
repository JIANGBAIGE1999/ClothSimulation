Cloth Sprite 
by Andrii Sudyn

----------

Online manual: http://ax23w4.com/devlog/clothsprite

Hi and thanks for choosing my asset! I 've tried to make it as
easy to use and self-explnatory as I can - each property
has a tooltip explaining what it does - but if something's
not working as expected, please read this file and be sure to
also check out an online manual. If you're having questions, 
you're always welcome to contact me using Twitter or email 
provided below.

If you like this asset, you can give it a rating here,
it helps a lot: http://u3d.as/1Wc3

And here's a guide to start using it.

----------

To add a Cloth Sprite object to you scene, go to:
GameObject > 2D Object > Cloth Sprite

To assign a sprite to it - drag a sprite into the "Sprite" field.

"Color" field lets you tint your sprite same way as any regular
Unity sprite.

"Fixed points" field lets you choose which points of cloth will
be fixed (immovable) in space.

"Resolution" field defines how many such points will be created
both horizontally and vertically. The cloth is a rectangular
grid that consist of these points, connected to each other by
constraint that keep them together.

Below you'll se a counter which indicates how many triangles
the mesh has. It's not that important to the performance, it's
just for curiosity, but the following two options are.

"Point connections" allows you to select from two ways to connect
the simulated cloth points to each other. First way is just
connecting every point to its neightbor on the left, right, top
and bottom. The second way also adds the diagonal connection.
The adventage of having diagonal connections is that the cloth
will spread out more, be less saggy, but since each connection
has to be calculated it doubles the number or those calculations.

"Computation passes" sets how many times each connection will be
recalculated. Recalculating them multiple times makes cloth more
firm. But as with previous option, it's a trade of. Together
these two options are defining how many calculations will be
performed each frame for this Cloth Sprite object.

"Mass" defines how much each simulated point of the cloth weights.

"Stiffness" defines how rigid those connections betweens points
are. It can't make them perfectly rigid but it can make them more
loose and relaxed. If you have diagonal connections enables you'll
also see "Stiffness (diagonal)" that does the same for diagonal
connections - these connections doesn't have to be that rigid.
Generally you don't want to set regular stiffness to be less
than diagonal - it stops looking like cloth.

Below this option you will see the status indicator. When
cloth sprite is not moving for a few seconds it enters the "sleep"
mode to stop calculating cloth physics.

"Impact force" is a number that multiplies the force, which is
applied to cloth when another 2D colliders touches it.
Cloth Sprite uses PolygonCollider2D in a trigger mode to detect
any colliders 2D that enter it and move the cloth based on
their size and speed. The impact only happens when something is
entering Cloth Sprite's collider. So the "Impact force" lets
you to make the power of that impact more subtle.

"Wind" checkbox allows you to ebnable constant wind for this
cloth sprite. It enables two options below.

"Wind directions" sets the direction of the wind in degrees.

"Wind force" sets the power of the wind.

"Sprite material" lets you toggle between unlit and lit
material. Both are based ond efault Unity sprite shaders.

Sorting layer and Order in layer fields work in the same 
way as they do for regular sprites.

That's it, now you're ready to start using Cloth Sprite.

My twitter: @ax23w4
Email: andrii.sudyn@gmail.com
My other assets: http://ax23w4.com/devlog/assets/
My games: http://ax23w4.com/devlog/games/