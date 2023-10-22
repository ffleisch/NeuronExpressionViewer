# NeuronExpressionViewer
We present a tool for the interactive real-time exploration of neuronal network simulations of plasticity changes in the human brain for the IEEE SciVis Contest 2023. To achieve visually pleasant results, we use the Unity game engine. We focus on displaying the changes and relations between the different attributes of the simulation in real time. A parser for infix expressions allows a high flexibility for the user to guide data exploration. The tool runs performantly and is extensible.


## Available Expressions
The following lists available functions and operators for the expression parser.
###Functions

Operator|Effect
---|---
`+´ | addition
`-` | substracion
`*` |multiplication
`\` |division
`^` |power
`sqrt(x)` |square root
`==` |equals, output 1f or 0f
`%` |float modulo
`map(x,a,b,c,d)` |map a float value linearly from the range (a,b) to (c,d)
`clip(x,l,h)`|clip a value x to the range from l to h
`grad(val)`|sample the selected gradient Texture from 0 to 1 and display the color
`rgb(r,g,b)`|output the rgb color made up of three given values in the range of 0 to 1
 
###Attributes

Attribute|Name|Description
---|---|---
uvx| Uv X |put the uv x coordinate on the stack
uvy| Uv Y |put the uv y coordinate on the stack
index| Neuron Index |put the index of the depicted neurton on the stack	
area| Area Index |put the area on the stack

fired|Fired Boolean|Did the neuron fire within the last sample step
fired_fraction|Fired Fraction|In Percent: Number of firings since the last sampling
activity|x|Electric Activity
dampening|Secondary Variable|Inhibition variable used for the firing model of Izhikevich
current_calcium|Calcium|Current calcium level
target_calcium|Target Calcium|Target calcium level
synaptic_input|Synaptic Input|Input electrical activity
background_input|Background Activity|Background noise electric activity input
grown_axons|Grown Axons|Number of currently grown axonal boutons
conneted_axons|Connected Axons|Number of current outgoing connections
grown_dendrites|Grown Excitatory Dendrites|Number of currently grown dendrite spines for excitatory connections
connected_dendrites|Connected Excitatory Dendrites|Number of incoming excitatory connections
