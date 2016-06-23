#-*-coding:utf-8-*-

import requests, json
import threading

url = 'http://oxfordhk.azure-api.net/academic/v1.0/evaluate?expr=%s&count=1000000&attributes=Id,RId,AA.AuId,AA.AfId,F.FId,C.CId,J.JId&subscription-key=f7cc29509a8443c5b3a5e56b0e38b5a6'

#2147152072
#https://oxfordhk.azure-api.net/academic/v1.0/evaluate?expr=Id=2332023333&count=10000&attributes=Id,RId,AA.AuId,F.FId,C.CId,J.JId&subscription-key=f7cc29509a8443c5b3a5e56b0e38b5a6
#https://oxfordhk.azure-api.net/academic/v1.0/evaluate?expr=Composite(AA.AuId=57898110)&count=10000&attributes=Id,RId,AA.AuId,F.FId,C.CId,J.JId&subscription-key=f7cc29509a8443c5b3a5e56b0e38b5a6


# First number
# 0 id -> id
# 1 Auid -> id
# 2 id -> Auid
# 3 Auid -> Auid

# Second number
# 1 1-hop
# 2 2-hop
# 3 3-hop

def comp0(dic1, dic2, maxLength, id1, id2, pre, bac, ans):
    try:
        if maxLength >= 2:
            Rid1 = dic1['RId']
            for item in Rid1:
                if item == id2:
                    ans.append(pre + [id1, id2] + bac)
                    break
    except:
        pass

    try:
        if maxLength >= 3:
            F1 = dic1['F']
            F2 = dic2['F']
            tmp = []
            for item in F1:
                tmp.append(item['FId'])
            for item in F2:
                if item['FId'] in tmp:
                    ans.append(pre + [id1, item['FId'], id2] + bac)
    except:
        pass

    try:
        if maxLength >= 3:
            J1 = dic1['J']
            J2 = dic2['J']
            if J1['JId'] == J2['JId']:
                ans.append(pre + [id1, J1['JId'], id2] + bac)
    except:
        pass

    try:
        if maxLength >= 3:
            C1 = dic1['C']
            C2 = dic2['C']
            if C1['CId'] == C2['CId']:
                ans.append(pre + [id1, C1['CId'], id2] + bac)
    except:
        pass

    try:
        if maxLength >= 3:
            AA1 = dic1['AA']
            AA2 = dic2['AA']
            tmp = []
            for item in AA1:
                tmp.append(item['AuId'])
            for item in AA2:
                if item['AuId'] in tmp:
                    ans.append(pre + [id1, item['AuId'], id2] + bac)
    except:
        pass

def comp1(dic1, dic2, maxLength, AuId1, id2, pre, bac, ans):
    try:
        if maxLength >= 2:
            Id = dic1['Id']
            if Id == dic2['Id']:
                ans.append(pre + [AuId1, id2] + bac)
    except:
        pass

def comp2(dic1, dic2, maxLength, id1, AuId2, pre, bac, ans):
    try:
        if maxLength >= 2:
            AA1 = dic1['AA']
            for item in AA1:
                if item['AuId'] == AuId2:
                    ans.append(pre + [id1, AuId2] + bac)
                    break
    except:
        pass

def comp3_1(dic1, dic2, maxLength, AuId1, pre, bac, ans):
    try:
        if maxLength >= 3:
            AA1 = dic1['AA']
            AA2 = dic2['AA']
            AfId1 = None
            for item in AA1:
                if item['AuId'] == AuId1 and 'AfId' in item:
                    AfId1 = item['AfId']
                    break
            if AfId1 != None:
                for item in AA2:
                    if 'AfId' in item and item['AfId'] == AfId1:
                        ans.append(pre + [AuId1, AfId1, item['AuId']] + bac)

    except:
        pass

def comp3_2(dic1, dic2, maxLength, AuId2, pre, bac, ans):
    try:
        if maxLength >= 3:
            AA1 = dic1['AA']
            AA2 = dic2['AA']
            AfId2 = None
            for item in AA2:
                if item['AuId'] == AuId2 and 'AfId' in item:
                    AfId2 = item['AfId']
                    break
            if AfId2 != None:
                for item in AA1:
                    if 'AfId' in item and item['AfId'] == AfId2:
                        ans.append(pre + [item['AuId'], AfId2, AuId2] + bac)

    except:
        pass

def comp3(dic1, dic2, maxLength, AuId1, AuId2, pre, bac, ans):
    try:
        if maxLength >= 3:
            AA1 = dic1['AA']
            AA2 = dic2['AA']

            AfId2 = set()
            for item in AA2:
                if item['AuId'] == AuId2 and 'AfId' in item:
                    AfId2.add(item['AfId'])
   
            for i in AfId2:
                for item in AA1:
                    if 'AfId' in item and item['AfId'] == i and item['AuId'] == AuId1:
                        ans.append(pre + [AuId1, i, AuId2] + bac)
            Id1 = dic1['Id']
            Id2 = dic2['Id']
            if Id1 == Id2:
                ans.append(pre + [AuId1, Id1, AuId2] + bac)
    except:
        pass


def getOr(l, r, arr):
    if l == r:
        return 'Id=%d'%arr[l]
    if r - l == 1:
        return 'Or(Id=%d,Id=%d)'%(arr[l], arr[r])
    mid = (l + r) / 2
    lp = getOr(l, mid, arr)
    rp = getOr(mid + 1, r, arr)
    return 'Or(%s,%s)'%(lp, rp)

def getOr2(l, r, arr):
    if l == r:
        return 'Composite(AA.AuId=%d)'%arr[l]
    if r - l == 1:
        return 'Or(Composite(AA.AuId=%d),Composite(AA.AuId=%d))'%(arr[l], arr[r])
    mid = (l + r) / 2
    lp = getOr2(l, mid, arr)
    rp = getOr2(mid + 1, r, arr)
    return 'Or(%s,%s)'%(lp, rp)

def thread0_1(urlx, tmps, id1, id2, ans):
    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    for item in entitiesx:
        RIdx = item['RId']
        for i in RIdx:
            if i in tmps:
                ans.append([id1, item['Id'], i, id2])

def thread0_0(urlx, dic2, maxLength, id1, id2, ids, ans):
    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    for item in entitiesx:
        comp0(item, dic2, maxLength, item['Id'], id2, [id1], [], ans)
        ids.add(item['Id'])

# id->id
def search_0(id1, id2, en1, en2, ans):
    entities1 = en1[0]
    entities2 = en2[0]

    #id->id, id->Auid->id, id->Jid->id, id->Fid->id, id->Cid->id
    comp0(entities1, entities2, 4, id1, id2, [], [], ans)

    #(id->id)->id
    urlx = url%('RId=%d'%id2)
    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    tmps = set()
    for item in entitiesx:
        tmps.add(item['Id'])

    threads = []
    for item in entitiesx:
        t = threading.Thread(target=comp0, args=(entities1, item, 3, id1, item['Id'], [], [id2], ans))
        threads.append(t)
        t.setDaemon(True)
        t.start()

    RId1 = entities1['RId']
    if len(RId1) == 0:
        return
    else:
        Query = []
        l = 0
        r = 49
        while l < len(RId1):
            Query.append(getOr(l, min(r, len(RId1) - 1), RId1))
            l += 50
            r += 50

    #id->(id->id)
    ids = set()

    for item in Query:
        t = threading.Thread(target=thread0_0, args=(url%item, entities2, 3, id1, id2, ids, ans))
        threads.append(t)
        t.setDaemon(True)
        t.start()

    if len(threads) != 0:
        for t in threads:
            t.join()

    ids = list(ids)
    if len(ids) == 0:
        return
    else:
        Query = []
        l = 0
        r = 49
        while l < len(ids):
            Query.append(getOr(l, min(r, len(ids) - 1), ids))
            l += 50
            r += 50

        for item in Query:
            t = threading.Thread(target=thread0_1, args=(url%(item), tmps, id1, id2, ans))
            threads.append(t)
            t.setDaemon(True)
            t.start()

    if len(threads) != 0:
        for t in threads:
            t.join()

def thread1_0(item, dic2, AuId, id, ans):
    for j in dic2:
        comp3_1(item, j, 3, AuId, [], [id], ans)

def thread1_1(urlx, dic1, AuId, id,  ans):
    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    RIds = set()
    for item in entitiesx:
        if id in item['RId']:
            for i in dic1:
                if item['Id'] in i['RId']:
                    ans.append([AuId, i['Id'], item['Id'], id])

def thread1_2(urlx, AfId1, AuId, id, AuIds, ans):
    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    for item in entitiesx:
        AAx = item['AA']
        for i in AAx:
            if 'AfId' in i and i['AfId'] in AfId1 and i['AuId'] in AuIds:
                ans.append([AuId, i['AfId'], i['AuId'], id])


# Auid->id
def search_1(AuId, id, entities1, entities2, ans):
    if len(entities1) == 0 or len(entities2) == 0:
        return

    threads = []

    #AuId->id
    for item in entities1:
        t = threading.Thread(target=comp1, args=(item, entities2[0], 4, AuId, id, [], [], ans))
        t2 = threading.Thread(target=comp0, args=(item, entities2[0], 3, item['Id'], id, [AuId], [], ans))
        threads.append(t)
        threads.append(t2)
        t.setDaemon(True)
        t2.setDaemon(True)
        t.start()
        t2.start()

    #AuId->AfId->AuId->id
    for i in entities1:
        t = threading.Thread(target=thread1_0, args=(i, entities2, AuId, id, ans))
        threads.append(t)
        t.setDaemon(True)
        t.start()

    ids = set()
    for item in entities1:
        RId1 = item['RId']
        for i in RId1:
            ids.add(i)

    ids = list(ids)
    if len(ids) == 0:
        pass
    else:
        Query = []
        l = 0
        r = 49
        while l < len(ids):
            Query.append(getOr(l, min(r, len(ids) - 1), ids))
            l += 50
            r += 50

        for item in Query:
            t = threading.Thread(target=thread1_1, args=(url%(item), entities1, AuId, id,  ans))
            threads.append(t)
            t.setDaemon(True)
            t.start()

    AuIds = []
    for item in entities2:
        for i in item['AA']:
            AuIds.append(i['AuId'])

    AfId1 = set()
    for item in entities1:
        AA1 = item['AA']
        for i in AA1:
            if i['AuId'] == AuId:
                if 'AfId' in i:
                    AfId1.add(i['AfId'])

    Query = []
    l = 0
    r = 29
    while l < len(AuIds):
        Query.append(getOr2(l, min(r, len(AuIds) - 1), AuIds))
        l += 30
        r += 30

    for item in Query:
        t = threading.Thread(target=thread1_2, args=(url%(item), AfId1, AuId, id, AuIds, ans))
        threads.append(t)
        t.setDaemon(True)
        t.start()

    if len(threads) != 0:
        for t in threads:
            t.join()


def thread2_0(urlx, id, AuId, tmps, ans):
    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    for item in entitiesx:
        RIdx = item['RId']
        for i in RIdx:
            if i in tmps:
                ans.append([id, item['Id'], i, AuId])

def thread2_1(urlx, AfId2, id, AuId, AuIds, ans):

    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    for item in entitiesx:
        AAx = item['AA']
        for i in AAx:
            if 'AfId' in i and i['AfId'] in AfId2 and i['AuId'] in AuIds:
                ans.append([id, i['AuId'], i['AfId'], AuId])

# id->Auid
def search_2(id, AuId, entities1, entities2, ans):
    if len(entities1) == 0 or len(entities2) == 0:
        return

    #id->AuId
    for item in entities2:
        comp2(entities1[0], item, 4, id, AuId, [], [], ans)

    #id->AuId->AfId->AuId
    for i in entities1:
        for j in entities2:
            comp3_2(i, j, 3, AuId, [id], [], ans)

    tmps = set()
    #id->id->AuId
    for item in entities2:
        Idx = item['Id']
        tmps.add(Idx)
        comp0(entities1[0], item, 3, id, Idx, [], [AuId], ans)
    
    RId1 = entities1[0]['RId']
    threads = []
    if len(RId1) == 0:
        pass
    else:
        Query = []
        l = 0
        r = 49
        while l < len(RId1):
            Query.append(getOr(l, min(r, len(RId1) - 1), RId1))
            l += 50
            r += 50
        for item in Query:
            t = threading.Thread(target=thread2_0, args=(url%item, id, AuId, tmps, ans))
            threads.append(t)
            t.setDaemon(True)
            t.start()

    
    AuIds = []
    for item in entities1:
        for i in item['AA']:
            AuIds.append(i['AuId'])

    AfId2 = set()
    for item in entities2:
        AA2 = item['AA']
        for i in AA2:
            if i['AuId'] == AuId:
                if 'AfId' in i:
                    AfId2.add(i['AfId'])

    Query = []
    l = 0
    r = 29
    while l < len(AuIds):
        Query.append(getOr2(l, min(r, len(AuIds) - 1), AuIds))
        l += 30
        r += 30
    for item in Query:
        t = threading.Thread(target=thread2_1, args=(url%item, AfId2, id, AuId, AuIds, ans))
        threads.append(t)
        t.setDaemon(True)
        t.start()

    if len(threads) != 0:
        for t in threads:
            t.join()


def thread3_0(urlx, AuId1, AuId2, tmps, ans):
    resx = requests.get(urlx)
    datax = json.loads(resx.text)
    entitiesx = datax['entities']
    for item in entitiesx:
        RIdx = item['RId']
        for i in RIdx:
            if i in tmps:
                ans.append([AuId1, item['Id'], i, AuId2])

# Auid->Auid
def search_3(AuId1, AuId2, entities1, entities2, ans):
    if len(entities1) == 0 or len(entities2) == 0:
        return

    #AuId->*>AuId
    for i in entities1:
        for j in entities2:
            comp3(i, j, 4, AuId1, AuId2, [], [], ans)

    tmps = set()
    for item in entities2:
        tmps.add(item['Id'])

    if len(entities1) == 0:
        return
    else:
        ids = []
        for item in entities1:
            ids.append(item['Id'])
        Query = []
        l = 0
        r = 49
        while l < len(ids):
            Query.append(getOr(l, min(r, len(ids) - 1), ids))
            l += 50
            r += 50
    threads = []
    for item in Query:
        t = threading.Thread(target=thread3_0, args=(url%item, AuId1, AuId2, tmps, ans))
        threads.append(t)
        t.setDaemon(True)
        t.start()

    if len(threads) != 0:
        for t in threads:
            t.join()

