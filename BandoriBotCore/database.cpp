#include <cstdio>
#include <vector>
#include <cstring>
#include <mutex>

extern "C"
{
    struct Cache{
        int index;
        char* buf;
        Cache(int index, char* buf) : index(index), buf(buf) {}
    };

    int RecordLength();
    void AddRecord(long qq, long group, long timestamp, const wchar_t *message);
    Cache* CacheIndex(int start, int end);
    bool RecordContains(const Cache* cache, int index, const wchar_t *substr);
    void ReadRecord(const Cache* cache, int index, long *qq, long *group, long *timestamp, wchar_t **message);
    void FreeCache(Cache* cache);
    void OpenFile(const char *datafile, const char *indexfile);
    void FlushFile();
    void CloseFile();
}

static FILE *fpdata, *fpindex;
static int curIndex;
static std::vector<long> indexCache;
static std::mutex mdata;

int RecordLength()
{
    return indexCache.size();
}

void CloseFile()
{
    fclose(fpdata);
    fclose(fpindex);
}

void FlushFile()
{
    fflush(fpdata);
    fflush(fpindex);
}

static char databuf[4 << 20], indexbuf[4 << 20];
void OpenFile(const char *datafile, const char *indexfile)
{
    fpdata = fopen(datafile, "ab+");
    fpindex = fopen(indexfile, "ab+");

    //setvbuf(fpdata, databuf, _IOFBF, 4 << 20);
    //setvbuf(fpindex, indexbuf, _IOFBF, 4 << 20);
    fseek(fpindex, 0, SEEK_END);
    curIndex = ftell(fpindex);
    int n = curIndex / sizeof(long);
    long *buf = new long[n];
    fseek(fpindex, 0, SEEK_SET);
    fread(buf, curIndex, 1, fpindex);
    for (int i = 0; i < n; ++i)
        indexCache.push_back(buf[i]);
    fseek(fpdata, 0, SEEK_END);
    curIndex = ftell(fpdata);
    printf("total %d records\n", n);
    delete[] buf;
    fseek(fpindex, 0, SEEK_END);
}

Cache* CacheIndex(int start, int end)
{
    int spos = indexCache[start], epos;
    if (end >= indexCache.size()) epos = curIndex;
    else epos = indexCache[end];
    char *buf = new char[epos - spos];

    mdata.lock();
    fseek(fpdata, spos, SEEK_SET);
    fread(buf, epos - spos, 1, fpdata);
    mdata.unlock();

    return new Cache(start, buf);
}

void FreeCache(Cache* cache)
{
    delete[] cache->buf;
    delete cache;
}

void AddRecord(long qq, long group, long timestamp, const wchar_t *message)
{
    int n = (wcslen(message) + 1) * sizeof(wchar_t);
    int rlen = sizeof(long) + sizeof(long) + sizeof(long) + n;
    char* record = new char[rlen];
    indexCache.push_back(curIndex);

    *(long*)record = qq;
    *(long*)(record + sizeof(long)) = group;
    *(long*)(record + sizeof(long) + sizeof(long)) = timestamp;
    memcpy(record + sizeof(long) + sizeof(long) + sizeof(long), message, n);

    mdata.lock();
    fseek(fpdata, 0, SEEK_END);
    fwrite(record, rlen, 1, fpdata);
    mdata.unlock();

    fwrite(&curIndex, 4, 1, fpindex);

    curIndex += rlen;
    delete[] record;
}

void ReadRecord(const Cache* cache, int index, long *qq, long *group, long *timestamp, wchar_t **message)
{
    char *record = cache->buf + indexCache[index] - indexCache[cache->index];
    *qq = *(long*)record;
    *group = *(long*)(record + sizeof(long));
    *timestamp = *(long*)(record + sizeof(long) + sizeof(long));
    *message = (wchar_t *)(record + sizeof(long) + sizeof(long) + sizeof(long));
}

bool RecordContains(const Cache* cache, int index, const wchar_t *substr)
{
    char *record = cache->buf + indexCache[index] - indexCache[cache->index];
    wchar_t *message = (wchar_t *)(record + sizeof(long) + sizeof(long) + sizeof(long));

    return wcsstr(message, substr) != NULL;
}