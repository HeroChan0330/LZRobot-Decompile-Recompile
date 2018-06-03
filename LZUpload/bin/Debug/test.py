#!/usr/bin/python3 
import ctypes 
import time 
import lzai 
ll = ctypes.cdll.LoadLibrary 
Lmst = ll("./libpycall.so") 

open('web/www/log.txt','r') as fp:
  fp.write('hello')

for count in range(10):
  Lmst.OpenHeadLight()
  time.sleep(1)
  Lmst.CloseHeadLight()
  time.sleep(1)