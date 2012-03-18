﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Terminals.Data.DB
{
    /// <summary>
    /// SQL persisted favorites container
    /// </summary>
    internal class Favorites : IFavorites
    {
        private DataBase dataBase;
        private Groups groups;
        private DataDispatcher dispatcher;

        internal Favorites(DataBase dataBase, Groups groups, DataDispatcher dispatcher)
        {
            this.dataBase = dataBase;
            this.groups = groups;
            this.dispatcher = dispatcher;
        }

        IFavorite IFavorites.this[Guid favoriteId]
        {
            get { return this.dataBase.GetFavoriteByGuid(favoriteId); }
        }

        IFavorite IFavorites.this[string favoriteName]
        {
            get
            {
                return this.dataBase.Favorites
                  .FirstOrDefault(favorite => favorite.Name
                      .Equals(favoriteName, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public void Add(IFavorite favorite)
        {
            AddToDataBase(favorite);
            this.dispatcher.ReportFavoriteAdded(favorite);
            this.dataBase.SaveImmediatelyIfRequested();
        }

        private void AddToDataBase(IFavorite favorite)
        {
            this.dataBase.AddObject("Favorites", favorite);
        }

        public void Add(List<IFavorite> favorites)
        {
            AddAllToDatabase(favorites);
            this.dispatcher.ReportFavoritesAdded(favorites);
            this.dataBase.SaveImmediatelyIfRequested();
        }

        private void AddAllToDatabase(List<IFavorite> favorites)
        {
            foreach (var favorite in favorites)
            {
                AddToDataBase(favorite);
            }
        }

        public void Update(IFavorite favorite)
        {
            var candidate = favorite as Favorite;
            if(candidate.EntityState != EntityState.Modified)
                return;

            SaveAndReportFavoriteUpdated(favorite);
        }

        public void UpdateFavorite(IFavorite favorite, List<IGroup> groups)
        {
            List<IGroup> addedGroups = this.groups.AddToDatabase(groups);
            List<IGroup> missingGroups = ListsHelper.GetMissingSourcesInTarget(groups, favorite.Groups);
            List<IGroup> redundantGroups = ListsHelper.GetMissingSourcesInTarget(favorite.Groups, groups);
            
            Data.Favorites.AddIntoMissingGroups(favorite, missingGroups);
            Data.Groups.RemoveFavoritesFromGroups(new List<IFavorite> { favorite }, redundantGroups);
            List<IGroup> removedGroups = this.groups.DeleteEmptyGroupsFromDataBase();

            this.dispatcher.ReportGroupsRecreated(addedGroups, removedGroups);
            SaveAndReportFavoriteUpdated(favorite);
        }

        private void SaveAndReportFavoriteUpdated(IFavorite favorite)
        {
            this.dispatcher.ReportFavoriteUpdated(favorite);
            this.dataBase.SaveImmediatelyIfRequested();
        }

        public void Delete(IFavorite favorite)
        {
            DeleteFavoriteFromDatabase(favorite);
            this.dispatcher.ReportFavoriteDeleted(favorite);
            this.dataBase.SaveImmediatelyIfRequested();
        }

        public void Delete(List<IFavorite> favorites)
        {
            DeleteFavoritesFromDatabase(favorites);
            this.dispatcher.ReportFavoritesDeleted(favorites);
            this.dataBase.SaveImmediatelyIfRequested();
        }

        private void DeleteFavoritesFromDatabase(List<IFavorite> favorites)
        {
            foreach (var favorite in favorites)
            {
                DeleteFavoriteFromDatabase(favorite);
            }
        }

        private void DeleteFavoriteFromDatabase(IFavorite favorite)
        {
            this.dataBase.Favorites.DeleteObject(favorite as Favorite);
        }

        public SortableList<IFavorite> ToList()
        {
            IEnumerable<IFavorite> favorites = GetFavorites();
            return new SortableList<IFavorite>(favorites);
        }

        private IEnumerable<IFavorite> GetFavorites()
        {
            // to list because Linq to entities allowes only cast to primitive types
            return this.dataBase.Favorites.ToList();
        }

        public SortableList<IFavorite> ToListOrderedByDefaultSorting()
        {
            return Data.Favorites.OrderByDefaultSorting(this.ToList());
        }

        public void ApplyCredentialsToAllFavorites(List<IFavorite> selectedFavorites, ICredentialSet credential)
        {
            Data.Favorites.ApplyCredentialsToFavorites(selectedFavorites, credential);
            SaveAndReportFavoritesUpdated(selectedFavorites);
        }

        private void SaveAndReportFavoritesUpdated(List<IFavorite> selectedFavorites)
        {
            this.dispatcher.ReportFavoritesUpdated(selectedFavorites);
            this.dataBase.SaveImmediatelyIfRequested();
        }

        public void SetPasswordToAllFavorites(List<IFavorite> selectedFavorites, string newPassword)
        {
            Data.Favorites.SetPasswordToFavorites(selectedFavorites, newPassword);
            SaveAndReportFavoritesUpdated(selectedFavorites);
        }

        public void ApplyDomainNameToAllFavorites(List<IFavorite> selectedFavorites, string newDomainName)
        {
            Data.Favorites.ApplyDomainNameToFavorites(selectedFavorites, newDomainName);
            SaveAndReportFavoritesUpdated(selectedFavorites);
        }

        public void ApplyUserNameToAllFavorites(List<IFavorite> selectedFavorites, string newUserName)
        {
            Data.Favorites.ApplyUserNameToFavorites(selectedFavorites, newUserName);
            SaveAndReportFavoritesUpdated(selectedFavorites);
        }

        #region IEnumerable members

        public IEnumerator<IFavorite> GetEnumerator()
        {
            return this.GetFavorites()
              .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
