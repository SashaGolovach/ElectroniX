from element import Element
from calculation_lab import calculate
from collections import defaultdict
from pickle import load, dump
from copy import deepcopy

class Scheme(object):
    
    def __init__(self, ids=None, emf=None, adj=None):
        '''
        V - number of points
        ids - dict ID : Element (list of edges)
        adj - defaultdict point: list of Elements
        counter - is used for id assignment
        '''
        self.counter = max(ids) if ids else 0
        self.ids = ids or {} # already existing or empty
        self.emf = emf or {}
        self.adj = adj or defaultdict(list) 
        self.V = 0

    def points(self, id1, pos1, id2, pos2):
        '''returns two points, given two elements with id1 and id2 and
           positions of points relatively to the elements'''
        e1 = self.ids[id1]
        e2 = self.ids[id2]
        first = e1.left if pos1 == 'l' else e1.right
        second = e2.left if pos2 == 'l' else e2.right
        return [first, second]

    def updateI(self):
        '''calculate the current on every element'''
        f = open("scheme.txt", "w")
        f.write(str(self.adj))
        calculate(self.ids, self.adj, self.V)
        cop1 = deepcopy(self)
        cop2 = deepcopy(self)
        for id, e in cop2.ids.items():
            if e.kind == 'lamp' and e.U*e.I > e.critP:
                cop1.removeElement(id)
        calculate(cop1.ids, cop1.adj, cop1.V)
        for id, e in cop2.ids.items():
            if e.kind == 'lamp' and e.U*e.I > e.critP:
                self.setProperty(id, 'I', 0)
                self.setProperty(id, 'U', 0)
                self.ids[id].state = False
            else:
                self.setProperty(id, 'I', cop1.ids[id].I)
                self.setProperty(id, 'U', cop1.ids[id].U)
        f.write(str(self.adj))
        f.close()
        #calculate(self.ids, self.adj, self.V)

    def addElement(self, kind, e=None):
        '''adds a new element'''
        self.counter += 1
        if e:
            self.ids[self.counter] = e
            self.adj[e.left].append([e.near(e.left), e.R, e.E])
            self.adj[e.right].append([e.near(e.right), e.R, e.E])
        else:
            if kind == 'resistor' or kind == 'ammeter' or kind == 'voltmeter':
                e = Element(kind, self.V+1, self.V+2, 1.0, 0, self.counter)
            elif kind == 'lEMF':
                e = Element(kind, self.V+1, self.V+2, 1.0, 1.0, self.counter)
            elif kind == 'rEMF':
                e = Element(kind, self.V+1, self.V+2, 1.0, -1.0, self.counter)
            elif kind == 'lamp':
                e = Element(kind, self.V+1, self.V+2, 1.0, 0.0, self.counter)
            self.ids[self.counter] = e
            self.adj[self.V+1].append([self.V+2, e.R, e.E])
            self.adj[self.V+2].append([self.V+1, e.R, -e.E])
            self.V += 2
        return e.id

    def removeConnection(self, id1, pos):
        cop = self.ids.copy()
        point = self.ids[id1].right if pos == "r" else self.ids[id1].left
        for id in cop.keys():
            if id != id1:
                if cop[id].left == point or cop[id].right == point:
                    self.removeElement(id)

    def removeElement(self, id):
        '''removes an element'''
        r = self.ids.pop(id)
        for v, el in self.adj.items():
            if [r.near(v), r.R, r.E] in el:
                el.remove([r.near(v), r.R, r.E])
        
    def connectElement(self, id1, pos1, id2, pos2):
        '''connects two points of two resistors and
           returns the id of a new wire'''
        first, second = self.points(id1, pos1, id2, pos2)
        e = Element('wire', first, second, 0, 0, self.counter+1)
        self.addElement('wire', e)
        return e.id

    def setProperty(self, id, name, val):
        '''sets property with name 'name'
           ('R' - resistence, 'E' - EMF)
           of an element to value 'val' '''
        e = self.ids[id]
        self.adj[e.left].remove([e.right, e.R, e.E])
        self.adj[e.right].remove([e.left, e.R, -e.E])
        if name == 'R':
            e.R = val                
        elif name == 'E':
            e.E = val
        elif name == 'critP':
            e.critP = val
        elif name == 'I':
            e.I = val
        elif name == 'U':
            e.U = val
        self.adj[e.right].append([e.left, e.R, -e.E])
        self.adj[e.left].append([e.right, e.R, e.E])

    def save(self, filename):
        with open(filename, 'wb') as f:
            dump(self, f)
            
    def load(self, filename):
        with open(filename, 'rb') as f:
            return load(f)

    def cop(self):
        return deepcopy(self)

s = Scheme()
