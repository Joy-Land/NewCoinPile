#import "KeyChain.h"
@implementation RHKeyChain
static NSString * const kRHDictionaryKey = @"com.words.course.wordgame.dictionaryKey";
static NSString * const kRHKeyChainKey = @"com.words.course.wordgame.keychainKey";
RHKeyChain *rHKeyChain;

+(RHKeyChain *)SingleTon{
	if(rHKeyChain==nil)
	{
        rHKeyChain = [[RHKeyChain alloc] init];
	}
	return rHKeyChain;
}

- (NSMutableDictionary *)getKeychainQuery:(NSString *)service {
    return [NSMutableDictionary dictionaryWithObjectsAndKeys:
            (id)kSecClassGenericPassword,(id)kSecClass,
            service, (id)kSecAttrService,
            service, (id)kSecAttrAccount,
            (id)kSecAttrAccessibleAfterFirstUnlock,(id)kSecAttrAccessible,
            nil];
}

- (void)save:(NSString *)service data:(id)data {
    //Get search dictionary
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    //Delete old item before add new item
    SecItemDelete((CFDictionaryRef)keychainQuery);
    //Add new object to search dictionary(Attention:the data format)
    [keychainQuery setObject:[NSKeyedArchiver archivedDataWithRootObject:data] forKey:(id)kSecValueData];
    //Add item to keychain with the search dictionary
    SecItemAdd((CFDictionaryRef)keychainQuery, NULL);
}

- (id)load:(NSString *)service {
    id ret = nil;
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    //Configure the search setting
    //Since in our simple case we are expecting only a single attribute to be returned (the password) we can set the attribute kSecReturnData to kCFBooleanTrue
    [keychainQuery setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
    [keychainQuery setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
    CFDataRef keyData = NULL;
    if (SecItemCopyMatching((CFDictionaryRef)keychainQuery, (CFTypeRef *)&keyData) == noErr) {
        ret = [NSKeyedUnarchiver unarchiveObjectWithData:(__bridge NSData *)keyData];
    }
    if (keyData)
        CFRelease(keyData);
    return ret;
}

- (void)delete:(NSString *)service {
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:service];
    SecItemDelete((CFDictionaryRef)keychainQuery);
}

- (void)rhKeyChainSave:(NSString *)service {
    NSMutableDictionary *tempDic = [NSMutableDictionary dictionary];
    [tempDic setObject:service forKey:kRHDictionaryKey];
    [self save:kRHKeyChainKey data:tempDic];
}

- (char *)rhKeyChainLoad{
    NSMutableDictionary *tempDic = (NSMutableDictionary *)[self load:kRHKeyChainKey];
    return MakeStringCopy([[tempDic objectForKey:kRHDictionaryKey] UTF8String]);
}

- (char*)getIDFA {
	NSString *str = @"";
    if (@available(iOS 14, *)) {
    	str = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
    }
    else{
        if ([ASIdentifierManager sharedManager].advertisingTrackingEnabled){
    	    str = [[[ASIdentifierManager sharedManager] advertisingIdentifier] UUIDString];
    	}    		
    }		
    return MakeStringCopy([str UTF8String]);
}

extern "C" {
    // Helper method to create C string copy
    
    char* MakeStringCopy (const char* string)
    {
        
        if (string == NULL)
            return NULL;
        
        char* res = (char*)malloc(strlen(string) + 1);
        
        strcpy(res, string);
        
        return res;
        
    }
}

- (void)rhKeyChainDelete{
    [self delete:kRHKeyChainKey];
}
@end
