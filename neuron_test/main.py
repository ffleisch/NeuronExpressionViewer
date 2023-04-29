import numpy as np
from matplotlib import pyplot as plt




stimulus=[0 for x in range (1000)]+[10 for x in range(1000)]





def neuron_gradient(a,b,u,v,I):
    dv=0.04*v**2+5*v+140-u+I
    du=a*(b*v-u)
    return du,dv

step_size=0.01
num_steps=10
#simple newtons rule
def neuron_update(a,b,c,d,u,v,I):

    for i in range(num_steps):
        du,dv=neuron_gradient(a,b,u,v,I)
        u+=du*step_size
        v+=dv*step_size
        if(v>30):
            v=c
            u=u+d

    print(v)
    return u,v


a=0.02
b=0.2

#intrinsically bursting

#c=-55
#d=4

#chattering

c=-50
d=2

#initial state

u=0
v=0

voltages=[]
for I in stimulus:
    u,v=neuron_update(a,b,c,d,u,v,I)
    voltages.append(v)


plt.plot(voltages)
plt.plot(stimulus)
plt.show()