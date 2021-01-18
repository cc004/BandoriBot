#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <vector>
#include <Windows.h>

using namespace std;

#include "pcrd.h"
#include "wasm-rt-impl.h"


/* import: 'wasi_unstable' 'fd_write' */
u32(*Z_wasi_unstableZ_fd_writeZ_iiiii)(u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueCall' */
void (*Z_envZ_syscallZ2FjsZ2EvalueCallZ_viiiiiiiii)(u32, u32, u32, u32, u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueGet' */
void (*Z_envZ_syscallZ2FjsZ2EvalueGetZ_viiiiii)(u32, u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueIndex' */
void (*Z_envZ_syscallZ2FjsZ2EvalueIndexZ_viiiii)(u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueNew' */
void (*Z_envZ_syscallZ2FjsZ2EvalueNewZ_viiiiiii)(u32, u32, u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueSet' */
void (*Z_envZ_syscallZ2FjsZ2EvalueSetZ_viiiiii)(u32, u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueSetIndex' */
void (*Z_envZ_syscallZ2FjsZ2EvalueSetIndexZ_viiiii)(u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.stringVal' */
void (*Z_envZ_syscallZ2FjsZ2EstringValZ_viiiii)(u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueLength' */
u32(*Z_envZ_syscallZ2FjsZ2EvalueLengthZ_iiii)(u32, u32, u32);
/* import: 'env' 'syscall/js.valuePrepareString' */
void (*Z_envZ_syscallZ2FjsZ2EvaluePrepareStringZ_viiii)(u32, u32, u32, u32);
/* import: 'env' 'syscall/js.valueLoadString' */
void (*Z_envZ_syscallZ2FjsZ2EvalueLoadStringZ_viiiiii)(u32, u32, u32, u32, u32, u32);
/* import: 'env' 'syscall/js.finalizeRef' */
void (*Z_envZ_syscallZ2FjsZ2EfinalizeRefZ_viii)(u32, u32, u32);

int myhash(const char* text);

enum typeflag
{
    object = 1,
    string = 2,
    symbol = 3,
    function = 4,
};

class dataview {
public:
    wasm_rt_memory_t *mem;
    vector<const char*> set;
    vector<char*> gcpool;

    dataview() {
        set.push_back("nan");
        set.push_back("0");
        set.push_back("null");
        set.push_back("true");
        set.push_back("false");
        set.push_back("global");
        set.push_back("this");
    }
    unsigned getUint32(int addr, bool flag)
    {
        return *(unsigned*)(mem->data + addr);
    }

    void setUint32(int addr, unsigned val, bool flag)
    {
        *(unsigned*)(mem->data + addr) = val;
    }

    char getUint8(int addr)
    {
        return *(mem->data + addr);
    }

    void setUint8(int addr, char val)
    {
        *(mem->data + addr) = val;
    }

    void setFloat64(int addr, double val, bool flag)
    {
        *(double *)(mem->data + addr) = val;
    }

    char* loadString(int ptr, int len)
    {
        char* str = new char[len + 1];
        memcpy(str, mem->data + ptr, len);
        str[len] = '\0';
        return str;
    }
    
    void storeValue(int ptr, int val)
    {
        setFloat64(ptr, val, true);
    }

    const int nanHead = 0x7FF80000;

    void storeValue(int ptr, typeflag flag, int id)
    {
        setUint32(ptr + 4, nanHead | flag, true);
        setUint32(ptr, id, true);
    }

    void storeValue(int ptr, typeflag flag, char* identity)
    {
        setUint32(ptr + 4, nanHead | flag, true);
        setUint32(ptr, set.size(), true);
        set.push_back(identity);
        gcpool.push_back(identity);
    }

    void storeValue(int ptr, typeflag flag, const char* identity)
    {
        setUint32(ptr + 4, nanHead | flag, true);
        setUint32(ptr, set.size(), true);
        set.push_back(identity);
        gcpool.push_back(nullptr);
    }

    const char* getValue(int ptr)
    {
        return set[getUint32(ptr, true)];
    }

    ~dataview()
    {
        for (auto str : gcpool)
            delete[] str;
    }
};

dataview *view;

/* import: 'wasi_unstable' 'fd_write' */
u32 _Z_wasi_unstableZ_fd_writeZ_iiiii(u32 fd, u32 iovs_ptr, u32 iovs_len, u32 nwritten_ptr)
{
    int nwritten = 0;

    if (fd == 1) {
        for (int iovs_i = 0; iovs_i < iovs_len; iovs_i++) {
            int iov_ptr = iovs_ptr + iovs_i * 8; // assuming wasm32
            int ptr = view->getUint32(iov_ptr + 0, true);
            int len = view->getUint32(iov_ptr + 4, true);
            for (int i = 0; i < len; i++) {
                int c = view->getUint8(ptr + i);
                //printf("%c", c);
            }
        }
    }
    else {
        //printf("invalid file descriptor: %d", fd);
    }
    view->setUint32(nwritten_ptr, nwritten, true);
    return 0;
}

/* import: 'env' 'syscall/js.valueCall' */
void _Z_envZ_syscallZ2FjsZ2EvalueCallZ_viiiiiiiii(u32 ret_addr, u32 v_addr, u32 m_ptr, u32 m_len, u32 args_ptr, u32 args_len, u32 args_cap, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    auto method = view->loadString(m_ptr, m_len);

    //printf("valueCall:%s.%s\n", type, method);

    if (strcmp(method, "_makeFunction") == 0)
        view->storeValue(ret_addr, typeflag::function, 1);
    else if (strcmp(method, "myhash") == 0)
        view->storeValue(ret_addr, myhash(view->getValue(args_ptr)));

    view->setUint8(ret_addr + 8, 1);
    delete[] method;

}

/* import: 'env' 'syscall/js.valueGet' */
void _Z_envZ_syscallZ2FjsZ2EvalueGetZ_viiiiii(u32 retval, u32 v_addr, u32 p_ptr, u32 p_len, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    auto prop = view->loadString(p_ptr, p_len);
    // auto value = loadValue(v_addr);

    //printf("valueGet:%s.%s\n", type, prop);

    if (strcmp(prop, "id") == 0)
        view->storeValue(retval, 1);
    else if (strcmp(prop, "hostname") == 0)
        view->storeValue(retval, typeflag::string, "pcrdfans.com");
    else if (strcmp(prop, "host") == 0)
        view->storeValue(retval, typeflag::string, "pcrdfans.com");
    else
    {
        view->storeValue(retval, typeflag::object, prop);
        return;
    }

    delete[] prop;
}

const char *text, *nonce;
int hash;
const char* result;

/* import: 'env' 'syscall/js.valueIndex' */
void _Z_envZ_syscallZ2FjsZ2EvalueIndexZ_viiiii(u32 ret_addr, u32 v_addr, u32 i, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    //printf("valueIndex:%s[%d]\n", type, i);

    if (strcmp(type, "args") == 0)
    {
        if (i == 0)
            view->storeValue(ret_addr, typeflag::string, text);
        else if (i == 1)
            view->storeValue(ret_addr, typeflag::string, nonce);
        else if (i == 2)
            view->storeValue(ret_addr, ::hash);
    }
}


/* import: 'env' 'syscall/js.valueNew' */
void _Z_envZ_syscallZ2FjsZ2EvalueNewZ_viiiiiii(u32 ret_addr, u32 v_addr, u32 args_ptr, u32 args_len, u32 args_cap, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    //printf("valueNew:new %s()\n", type);
    view->storeValue(ret_addr, typeflag::object, type);
    view->setUint8(ret_addr + 8, 1);
}


/* import: 'env' 'syscall/js.valueSet' */
void _Z_envZ_syscallZ2FjsZ2EvalueSetZ_viiiiii(u32 v_addr, u32 p_ptr, u32 p_len, u32 x_addr, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    auto prop = view->loadString(p_ptr, p_len);

    //printf("valueSet:%s.%s\n", type, prop);

    if (strcmp(prop, "result") == 0)
        result = view->getValue(x_addr);

    delete[] prop;
}


/* import: 'env' 'syscall/js.valueSetIndex' */
void _Z_envZ_syscallZ2FjsZ2EvalueSetIndexZ_viiiii(u32 syscall, u32 js, u32 v_addr, u32 i, u32 x_addr)
{
    //printf("FUNCINFO:%s\n", __FUNCSIG__);
}


/* import: 'env' 'syscall/js.stringVal' */
void _Z_envZ_syscallZ2FjsZ2EstringValZ_viiiii(u32 ret_ptr, u32 value_ptr, u32 value_len, u32 syscall, u32 js)
{
    auto value = view->loadString(value_ptr, value_len);
    // auto value = loadValue(v_addr);

    //printf("stringVal:%s\n", value);

    view->storeValue(ret_ptr, typeflag::string, value);

}


/* import: 'env' 'syscall/js.valueLength' */
u32 _Z_envZ_syscallZ2FjsZ2EvalueLengthZ_iiii(u32 v_addr, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    //printf("valueLength:%s\n", type);

    if (strcmp(type, "args") == 0)
        return 3;
    return 0;
}


/* import: 'env' 'syscall/js.valuePrepareString' */
void _Z_envZ_syscallZ2FjsZ2EvaluePrepareStringZ_viiii(u32 ret_addr, u32 v_addr, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    //printf("prepareString:%s\n", type);

    view->storeValue(ret_addr, typeflag::object, type);
    view->setUint32(ret_addr + 8, strlen(type), true);
}

/* import: 'env' 'syscall/js.valueLoadString' */
void _Z_envZ_syscallZ2FjsZ2EvalueLoadStringZ_viiiiii(u32 v_addr, u32 slice_ptr, u32 slice_len, u32 slice_cap, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    //printf("loadString:%s\n", type);

    memcpy(view->mem->data + slice_ptr, type, min((unsigned)strlen(type), slice_cap));
}


/* import: 'env' 'syscall/js.finalizeRef' */
void _Z_envZ_syscallZ2FjsZ2EfinalizeRefZ_viii(u32 v_addr, u32 syscall, u32 js)
{
    auto type = view->getValue(v_addr);
    //printf("finalizeRef:%s\n", type);

}

void g_jmp_buf(wasm_rt_trap_t trap)
{
}

int calcHash(const char* text)
{
    unsigned _0x473e93, _0x5d587e;
    for (_0x473e93 = 0x1bf52, _0x5d587e = strlen(text); _0x5d587e; )
        _0x473e93 = 0x309 * _0x473e93 ^ text[--_0x5d587e];
    return _0x473e93 >> 0x3;
}

int myhash(const char* text)
{
    //printf("myhash:%s\n", text);
    unsigned _0x473e93, _0x5d587e;
    for (_0x473e93 = 0x202, _0x5d587e = strlen(text); _0x5d587e; )
        _0x473e93 = 0x72 * _0x473e93 ^ text[--_0x5d587e];
    return _0x473e93 >> 0x3;
}

wchar_t* trans(const char* ch, int type = CP_ACP) {
    int len = MultiByteToWideChar(type, 0, ch, -1, nullptr, 0);
    wchar_t* str = new wchar_t[len + 1];
    wmemset(str, 0, len + 1);
    MultiByteToWideChar(type, 0, ch, -1, str, len);
    return str;
}

char* trans(const wchar_t* wch, int type = CP_ACP) {
    int len = WideCharToMultiByte(type, 0, wch, -1, nullptr, 0, nullptr, nullptr);
    char* str = new char[len + 1];
    memset(str, 0, len + 1);
    WideCharToMultiByte(type, 0, wch, -1, str, len, nullptr, nullptr);
    return str;
}

extern "C"
{
    _declspec(dllexport) void __cdecl getSign(const char* text, const char* nonce, void (*callback)(char*))
    {
Z_wasi_unstableZ_fd_writeZ_iiiii = &_Z_wasi_unstableZ_fd_writeZ_iiiii;
        /* import: 'env' 'syscall/js.valueCall' */
Z_envZ_syscallZ2FjsZ2EvalueCallZ_viiiiiiiii = &_Z_envZ_syscallZ2FjsZ2EvalueCallZ_viiiiiiiii;
        /* import: 'env' 'syscall/js.valueGet' */
Z_envZ_syscallZ2FjsZ2EvalueGetZ_viiiiii = &_Z_envZ_syscallZ2FjsZ2EvalueGetZ_viiiiii;
        /* import: 'env' 'syscall/js.valueIndex' */
Z_envZ_syscallZ2FjsZ2EvalueIndexZ_viiiii = &_Z_envZ_syscallZ2FjsZ2EvalueIndexZ_viiiii;
        /* import: 'env' 'syscall/js.valueNew' */
Z_envZ_syscallZ2FjsZ2EvalueNewZ_viiiiiii = &_Z_envZ_syscallZ2FjsZ2EvalueNewZ_viiiiiii;
        /* import: 'env' 'syscall/js.valueSet' */
Z_envZ_syscallZ2FjsZ2EvalueSetZ_viiiiii = &_Z_envZ_syscallZ2FjsZ2EvalueSetZ_viiiiii;
        /* import: 'env' 'syscall/js.valueSetIndex' */
Z_envZ_syscallZ2FjsZ2EvalueSetIndexZ_viiiii = &_Z_envZ_syscallZ2FjsZ2EvalueSetIndexZ_viiiii;
        /* import: 'env' 'syscall/js.stringVal' */
Z_envZ_syscallZ2FjsZ2EstringValZ_viiiii = &_Z_envZ_syscallZ2FjsZ2EstringValZ_viiiii;
        /* import: 'env' 'syscall/js.valueLength' */
Z_envZ_syscallZ2FjsZ2EvalueLengthZ_iiii = &_Z_envZ_syscallZ2FjsZ2EvalueLengthZ_iiii;
        /* import: 'env' 'syscall/js.valuePrepareString' */
Z_envZ_syscallZ2FjsZ2EvaluePrepareStringZ_viiii = &_Z_envZ_syscallZ2FjsZ2EvaluePrepareStringZ_viiii;
        /* import: 'env' 'syscall/js.valueLoadString' */
Z_envZ_syscallZ2FjsZ2EvalueLoadStringZ_viiiiii = &_Z_envZ_syscallZ2FjsZ2EvalueLoadStringZ_viiiiii;
        /* import: 'env' 'syscall/js.finalizeRef' */
Z_envZ_syscallZ2FjsZ2EfinalizeRefZ_viii = &_Z_envZ_syscallZ2FjsZ2EfinalizeRefZ_viii;


        init();
        view = new dataview;
        view->mem = Z_memory;
        Z__startZ_vv();

        ::text = text;
        ::nonce = nonce;
        ::hash = calcHash(nonce);

        Z_resumeZ_vv();
        wchar_t *str = trans(result, CP_UTF8);
        char* ansi = trans(str, CP_ACP);
        callback(ansi);

        delete[] ansi;
        delete[] str;
        _finalize();
        delete view;
    }
}