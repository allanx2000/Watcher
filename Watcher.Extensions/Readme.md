# Watcher.Extensions #
This class library contains the base classes needed to create extensions.

## Building an Extension ##

To create an extension, the class library must contain a class that implements **AbstractProvider**, **AbstractItem** and **AbstractSource**.

### AbstractSource ###
The AbstractSource class holds the Source definition that your Provider will generate and use to retrieve updates from your data source.

The source is usually linked to an AbstractProvider implementation, so in it's constructor you need to provide the key of your Provider class. For example:

    public HundredZeroSource() : base("HundredZero", HundredZeroProvider.PROVIDER)
        
By default, Watcher.Core de/serializes all sources as a GenericSource, which provides a generic implementation for serialization and the common use case. However, the AbstractSource allows customization via virtual functions. SO IF you override any of these, you also need to expose the GenericSource constructor

        public GoodAnimeSource(GenericSource src) : base(src)

to your Provider's CastToSource implementation.

#### public virtual string GetDisplayName() ####
Returns the friendly name of you're Source. The default is: 

    ProviderID + " > " + SourceName

But if your source only should return SourceName since there can only be one instance, you would override this.

### AbstractItem ###
Doesn't really do anything at the moment... you need to extend this but it doesn't really have anything customizable function at this time.

The **ActionContent** is a string that contains Provider-specific information used to execute an action in your Provider's `DoAction`, like open a URL.

### AbstractProvider ###

#### GetMetaFields ####
These are the source configuration values that the Provider recognizes and would use. 

Currently this function is called by the WPF.Client to create the SourceEditor window

#### GetSourceOptions ####
This returns the Source configuration properties like whether it can only be created once, or has a URL field.

#### DoCreateNewSource ####
Validates the input data for creating a new Source and returns an instance of it, if validation passes.

You should add any custom metadata values in this function. 

And you should return your custom Source if needed, at this time.

#### GetNewItems ####
The Provider is responsible for retrieving updates. Watcher.Core route the Source to it's appropriate provider and call this function to get a list of new items.

You are not required to track if an item already exists as Watcher this should be taken care of in the DataStore.

#### DoAction ####
Performs the action based on the values on the AbstractItem.

## Other Classes ##
### AbstractDataStore ###
Do not use this. 

Currently Watcher.WPF.Client is tightly coupled to SQLiteDataStore but in theory it should be able to support different types of DataStores, just like it supports Extensions, and those would need to implement this class.

This functionality I guess would need to come from Watcher.Core but haven't really figured out how to get around platform-specific dependency issues.

### GenericSource ###
Basic implementation of AbstractSource

### SourceOptions ###
Struct used be the Provider to define customizable options for the Source it uses.

### MTObservableCollection ###
You should not need to use this. This is an observation Collection that supports multi-threaded calls. 

**Probably should fix these references as Extensions should not need to know this.**

It is for the Abstract classes and some references from WPFClient for ViewModel purposes.