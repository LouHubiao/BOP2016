#-*-coding:utf-8-*-

from flask import Flask, request, jsonify
from lib3 import *
import threading

app = Flask(__name__)
app.debug = True

@app.route('/', methods=['GET'])
def index():
    return 'hello'

def thread_request(url, entitiesx, id):
    res = requests.get(url)
    data = json.loads(res.text)
    entities = data['entities']
    if len(entities) == 0:
        entitiesx[id] = None
    elif len(entities) == 1 and not 'AA' in entities[0]:
        entitiesx[id] = None
    else:
        entitiesx[id] = entities

@app.route('/work', methods=['GET'])
def work():
    args = request.args
    id1 = int(args['id1'])
    id2 = int(args['id2'])
    ans = []
    
    entitiesx = [None, None, None, None]
    s0 = threading.Thread(target=thread_request, args=(url%('Id=%d'%id1), entitiesx, 0))
    s1 = threading.Thread(target=thread_request, args=(url%('Composite(AA.AuId=%d)'%id1), entitiesx, 1))
    s2 = threading.Thread(target=thread_request, args=(url%('Id=%d'%id2), entitiesx, 2))
    s3 = threading.Thread(target=thread_request, args=(url%('Composite(AA.AuId=%d)'%id2), entitiesx, 3))
    threads = [s0, s1, s2, s3]

    for t in threads:
        t.setDaemon(True)
        t.start()
    for t in threads:
        t.join()

    if entitiesx[0] != None and entitiesx[2] != None:
        print 1
        search_0(id1, id2, entitiesx[0], entitiesx[2], ans)
    elif entitiesx[0] != None and entitiesx[3] != None:
        print 2
        search_2(id1, id2, entitiesx[0], entitiesx[3], ans)
    elif entitiesx[1] != None and entitiesx[2] != None:
        print 3
        search_1(id1, id2, entitiesx[1], entitiesx[2], ans)
    elif entitiesx[1] != None and entitiesx[3] != None:
        print 4
        search_3(id1, id2, entitiesx[1], entitiesx[3], ans)
    else:
        return '[]'

    ts = set()
    res = []
    for item in ans:
        tmp = ''
        for i in item:
            tmp += '%d '%i
        if not tmp in ts:
            res.append(item)
            ts.add(tmp)

    import json
    return json.dumps(res)
    # return jsonify(count=len(res), entities=res)

if __name__ == '__main__':
    app.run('0.0.0.0', 8080)
