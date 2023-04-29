import matplotlib.pyplot as plt
import numpy as np
import scipy.fft
import sklearn.decomposition
from PyQt5.QtCore import dec
from sklearn import decomposition
from scipy import ndimage
from scipy.fft import fft,fftfreq
# excitatory neurons
num_exc = 800

# inhibtory neurons
num_inh = 200

a = np.zeros(num_exc + num_inh)
a[:num_exc] = 0.02
a[num_exc:] = 0.02 + 0.08 * np.random.random(num_inh)

b = np.zeros(num_exc + num_inh)
b[:num_exc] = 0.2
b[num_exc:] = 0.25 - 0.05 * np.random.random(num_inh)

re = np.random.random(num_exc)
c = np.zeros(num_exc + num_inh)
c[:num_exc] = -65 + 15 * re ** 2
c[num_exc:] = -65

d = np.zeros(num_exc + num_inh)
d[:num_exc] = 8 - 6 * re ** 2
d[num_exc:] = 2

num_total=num_exc+num_inh
S_o = np.zeros((num_total,num_total))

S_o[:, :num_exc] = 0.5 * np.random.random((num_total, num_exc))
S_o[:, num_exc:] = -1 * np.random.random((num_total, num_inh))

plt.imshow(S_o)
plt.show()


v=-65*np.ones(num_total)
u=b*v


time = 1000  # in ms
step_size = 0.5

voltages=[]
firings=[]
time_steps=np.arange(0, time, step_size)
facs=np.linspace(1,1,time_steps.shape[0])
for i,t in enumerate(time_steps):
    S=S_o*facs[i]


    I=np.zeros(num_total) # thalamic input
    I[:num_exc]= 5 * np.random.randn(num_exc)
    I[num_exc:]= 2 * np.random.randn(num_inh)

    fired=v>30

    firings.append(fired)

    v[fired]=c[fired]
    u[fired]+=d[fired]
    I+=np.sum(S[:,fired],axis=1)
    v=v+0.04*v**2+5*v+140-u+I
    u=u+a*(b*v-u)
    voltages.append(v[0])
    #print(facs[i])
plt.plot(voltages)
plt.show()
plt.imshow(np.array(firings).transpose(), cmap="gray")
plt.show()

firings_sum=np.sum(firings, axis=1, dtype=float)

#firings_sum=ndimage.gaussian_filter1d(firings_sum,3)
plt.plot(firings_sum)
plt.show()

freqs=fft(firings_sum)
#plt.plot(np.real(freqs))
#plt.plot(np.imag(freqs))
#plt.show()
N=time_steps.shape[0]

xf=fftfreq(N,step_size/1000)[:N//2]
plt.plot(xf,np.abs(freqs)[:N//2])
plt.show()

#pca=sklearn.decomposition.PCA(n_components=10)

#test=pca.fit(fireings).transform(fireings)
#plt.plot(test[:,0],test[:,1])

#plt.show()
